using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Interpreter;


public class Parser {
    // FunctionMap -> Dict with declared functions (they are global)
    // VariableMap -> Dict with global variables <string, List<VariableDeclaration>>
    //
    // We can use them to infer the type
    Lexer lexer;
    Token current_token;

    Token[] tokens;
    int index;

    // global
    public Context global_context;
    public Context local_context;

    public Parser(Lexer lexer, Context context = null) {
        this.index = 0;
        this.lexer = lexer;
        this.tokens = this.lexer.GetAllTokens();

        this.current_token = this.GetNextToken();

        if (context is null) {
            this.global_context = this.local_context = new Context();
        }
        else {
            this.global_context = this.local_context = context;
        }
    }

    public Token GetNextToken() {
        return this.tokens[this.index++];
    }

    public void MoveBack(int steps = 1) {
        //regresar al token anterior o a los anteriores si no matchea lo que esperabamos 

        this.index -= steps;
        this.current_token = this.tokens[index];
   }

   public Token Peek(int steps = 1) {
       if (this.index < 0 || (this.index > this.tokens.Length)) {
           // return EOF so it doesn't match anything (except, maybe ending whatever loop is using peek)
           return new Token(Tokens.EOF);
       }
       return this.tokens[this.index + steps];
   }

    public void Error(Exception exception) {
        Console.WriteLine($"Error parsing line {this.current_token.Line} col {this.current_token.column}");
        // XXX write last line
        Console.WriteLine(this.lexer.Text);
        for (int i = 0; i < this.current_token.column - 2; i++) {
            Console.Write(" "); 
        }
        Console.Write("^");
        Console.WriteLine(); 
        throw exception;
    }

    public void Eat(string token_type) {
        if (this.current_token.type == token_type) {
            this.current_token = this.GetNextToken();
        }
        else {
            this.Error(new SyntaxError($"Expected {token_type} found {this.current_token.type}."));
        }
    }

    public AST TypeFor(string name) {
        if (this.local_context.ContainsKey(name)) {
            return this.local_context[name];
        }
        // XXX builtins
        return null;
    }

    public void Add(string ast) {
        return;
    }

    public BlockNode Arguments() {
        if (this.current_token.type != Tokens.LPAREN) {
            return null;
        }

        this.Eat(Tokens.LPAREN);
        List<AST> args = new List<AST>();
        if (this.current_token.type == Tokens.RPAREN) {
            this.Eat(Tokens.RPAREN);
            return new BlockNode(new List<AST>());
        }

       args.Add(this.Expr());
       while (this.current_token.type == Tokens.COMMA) {
           this.Eat(Tokens.COMMA);
           args.Add(this.Expr());
       }
       this.Eat(Tokens.RPAREN);

       return new BlockNode(args);
    }

    public AST Namespace(string? name=null, bool local=false) {
        // XXX could be functiof of one argument
        // draw "loli"
        if (name is null) {
            name = this.current_token.val;

            this.Eat(Tokens.ID);
        }

        AST node;
        BlockNode args = this.Arguments();
        if (args is not null) {
            node = new Function(name, args);
        }
        else {
            VariableDeclaration declaration = (VariableDeclaration) this.TypeFor(name);
            Variable var_node = new Variable(name);
            node = var_node;
            var_node.declaration = declaration;
        }
        return node;
    }

    public Lambda Letin() {
        // meme lambda-like aberration with bizarre use-cases (forma enervante)
        this.Eat(Tokens.LET);
        var nodes = new List<AST>();
        // we want a separate context for local vars in the lambda
        this.local_context = this.global_context.Clone();
        while (this.current_token.type != Tokens.IN) {
            nodes.Add(this.ParseNode());
            this.Eat(Tokens.END);
        }
        this.Eat(Tokens.IN);

        var node = new Lambda(new BlockNode(nodes), this.Expr());

        // restore old context
        this.local_context = this.global_context;

        return node;
    }

    protected (string, string?) DeclareFirst(List<AST> variables, List<string> names) {
        // helper function to declare a variable
        // the first time it can have a type before the name 
        // so it's an special case
        //
        // we modify "variables" and "names"
        //
        // FIXME fix this shite

        string name = this.current_token.val;

        this.Eat(Tokens.ID);
        // check sequence
        SequenceLiteral seq = null;
        string type = null;
        if (this.current_token.type == Tokens.SEQUENCE_START) {
            seq = (SequenceLiteral) this.LiteralNode();
            // the first string is the type
            type = name;
            name = this.current_token.val;
            this.Eat(Tokens.ID);
        }
        else if (this.current_token.type == Tokens.ID) {
            // NOTE we duplicate this code because if a sequence was declared here 
            // it has to be followed by an ID or else is a syntax error
            
            // name
            // what we got before was the type
            type = name;
            name = this.current_token.val;
            this.Eat(Tokens.ID);
        }
        else if (this.current_token.type == Tokens.LPAREN) {
            // fug, it's a function
            return (name, null);
        }

        AST val = null;
        if (this.current_token.type == Tokens.ASSIGN) {
            // asignation
            this.Eat(Tokens.ASSIGN);
            val = this.Expr();
        }
        else if (type is not null) {
            // declaration
            // [type] {args} [name]; is translated to name = type(args)
            // where type is a function
            // thus
            Function fun = new Function(type, new BlockNode(new List<AST>{seq}));
            val = fun;

        }
        VariableDeclaration variable = new VariableDeclaration(name, val);
        variables.Add(variable);

        // add to context
        this.local_context[variable.name] = variable;

        names.Add(name);

        return (name, type);
    }

    public BlockNode Declaration() {
        List<string> names = new List<string>();
        List<AST> variables = new List<AST>();

        (string name, string type) = this.DeclareFirst(variables, names);
        if (this.current_token.type == Tokens.LPAREN) {
            // function
            return new BlockNode(new List<AST>{this.FunctionDecl(name)});
        }
        VariableDeclaration variable;


        AST val = null;
        while (this.current_token.type == Tokens.COMMA) {
            this.Eat(Tokens.COMMA);
            name = this.current_token.val;
            names.Add(name);

            this.Eat(Tokens.ID);

            if (this.current_token.type == Tokens.ASSIGN) {
                this.Eat(Tokens.ASSIGN);
                val = this.Expr();
            }
            else {
                if (type is null) {
                    // undefined
                    val = null;
                }
                else {
                    Function fun = new Function(type, new BlockNode(new List<AST>()));
                    val = fun;
                }
            }


            variable = new VariableDeclaration(name, val);
            variables.Add(variable);


            // add to context
            this.local_context[variable.name] = variable;
        }

        // sequence declaration
        // this doesn't modify the original seq
        // NOTE we already evaluated the first element so if there is not a second
        // one we can't do this; hence the null check
        // don't worry about the first one not being taken into account--the list with
        // all variables is passed to the function handling the special case (DeclareFirst)
        if (!(val is null) && (val.Type == AST<object>.SEQUENCE)) {
            SequenceLiteral seq = (SequenceLiteral) val;
            var sequence = seq.Clone();

            // we pass the reference to all the variables so when they are evaluated
            // we modify the index @ Sequence.val and give a different value each time
            Terms term = sequence.Val();
            int counter = 0;
            foreach(VariableDeclaration item in variables) {
                if (counter == (variables.Count() - 1)) {
                    // minus the last one
                    // pass a reference to the cloned sequence to the last var
                    item.expression = sequence;
                    break;
                }
                // reference to the term that will return a literal when evaluated
                item.expression = term;
                counter += 1;
            }
        }

        BlockNode multi_decl = new BlockNode(variables);

        return multi_decl;
   }

    public AST FunctionDecl(string name) {
        //string name = this.current_token.val;
        //this.Eat(Tokens.ID);
        BlockNode args = this.Arguments();

        if (this.current_token.type == Tokens.ASSIGN) {
            this.Eat(Tokens.ASSIGN);
            // args are variables so we can use them here too
            return new FunctionDeclaration(name, args, this.Expr());
        }
        // XXX normal fun
        return null;
    }

    public Conditional ConditionalStmt() {
        this.Eat(Tokens.IF);

        AST hypothesis = this.Expr();

        this.Eat(Tokens.THEN);

        AST thesis = this.Expr();

        this.Eat(Tokens.ELSE);

        AST antithesis = this.Expr();

        return new Conditional(
            hypothesis,
            thesis,
            antithesis
        );
    }

    public AST GetSequence() {        
        if (this.current_token.type == Tokens.SEQUENCE_END) {
            this.Eat(Tokens.SEQUENCE_END);
            // terms start == end => always undefined
            return new SequenceLiteral(new Terms(0, 0));
        }
        AST first = this.Expr();
        AST last = first;
        string Type = first.Type;
        List<AST> items = new List<AST>{first};

        while (this.current_token.type != Tokens.SEQUENCE_END && this.current_token.type != Tokens.EOF && this.current_token.type != Tokens.DOT) {
            // finite
            this.Eat(Tokens.COMMA);
            AST item = this.Expr();
            if (item.Type != Type || item.Type == "DYNAMIC" || first.Type == "DYNAMIC") {
                this.Error(
                    new TypeError(
                        $"Inconsistent types of elements for secuence. Expected {Type} found {item.Type}"
                    )
                );
            }
            items.Add(item);
        }

        // exited the loop because there were no COMMAs
        // infinite
        if (this.current_token.type == Tokens.DOT) {
            if (first != last) {
                this.Error(new SyntaxError("Too many starting points for sequence. Expected one element (found many)"));
            }
            if (Type != AST<object>.INTEGER) {
                this.Error(new TypeError($"Can not make an infinite sequence of {Type} (try using integers literals)"));
            }
            // first == last
            this.Eat(Tokens.DOT);
            this.Eat(Tokens.DOT);
            this.Eat(Tokens.DOT);

            int start = first.Eval(null);
            int? end;
            if (this.current_token.type == Tokens.SEQUENCE_END) {
                end = null;
            }
            else {
                if (this.current_token.type != Tokens.INTEGER) {
                    this.Error(new TypeError($"Range end must be an integer. Found {this.current_token.type}"));
                }
                end = this.LiteralNode().Eval(null);
            }

            Terms term_fin = new Terms(start, end);

            this.Eat(Tokens.SEQUENCE_END);

            return new SequenceLiteral(term_fin);
        }

        // finite 
        // we have the items at "items"

        Terms term = new Terms(items);

        this.Eat(Tokens.SEQUENCE_END);

        return new SequenceLiteral(term);
    }

    public AST LiteralNode() {
        Token token = this.current_token;
        if (token.type == Tokens.STRING) {
            this.Eat(Tokens.STRING);
            return new StringLiteral(token.val);
        }
        else if (token.type == Tokens.INTEGER) {
            this.Eat(Tokens.INTEGER);
            return new IntLiteral(Convert.ToInt32(token.val));
        }
        else if (token.type == Tokens.FLOAT) {
            this.Eat(Tokens.FLOAT);
            return new FloatLiteral((float) Math.Round(Convert.ToSingle(token.val), 4));
        }
        else if (token.type == Tokens.SEQUENCE_START) {
            this.Eat(Tokens.SEQUENCE_START);
            return this.GetSequence();
        }

        // else
        this.Error(new SyntaxError($"Invalid literal {token.val} {token.type}"));
        return null;
    }

    public AST Term() {
        AST node = this.Factor();

        while (Lexer.OPERATIONS.Contains(this.current_token.type)) {
            Token token = this.current_token;
            Type ast = null;
            if (token.type == Tokens.MULT) {
                this.Eat(Tokens.MULT);
                ast = typeof(Mult);
            }
            else if (token.type == Tokens.DIV) {
                this.Eat(Tokens.DIV);
                ast = typeof(Division);
            }
            else if (token.type == Tokens.MODULO) {
                this.Eat(Tokens.MODULO);
                ast = typeof(Modulo);
            }
            else if (token.type == Tokens.EXP) {
                this.Eat(Tokens.EXP);
                ast = typeof(Exp);
            }

            AST[] args = new AST[] {node, this.Factor()};

            node = (AST) Activator.CreateInstance(
              type: ast,
              args: args
            );
        }

        return node;
    }

    public AST Factor() {
        Token token = this.current_token;
        AST node = null;

        // check unary first
        if (token.type == Tokens.MINUS) {
            this.Eat(Tokens.MINUS);
            node = new ChangeSign(this.Term());
        }
        else if (token.type == Tokens.NOT) {
            this.Eat(Tokens.NOT);
            node = new Negate(this.Term());
        }
        else if (Lexer.LITERALS.Contains(token.type)) {
            node = this.LiteralNode();
        }
        else if (token.type == Tokens.LET) {
            node = this.Letin();
        }
        else if (token.type == Tokens.LPAREN) {
            this.Eat(Tokens.LPAREN);
            node = this.Expr();
            this.Eat(Tokens.RPAREN);
        }
        else if (token.type == Tokens.IF) {
            node = this.ConditionalStmt();
        }
        else if (token.type == Tokens.ID) {
            node = this.Namespace();
        }
        else {
            this.Error(new SyntaxError($"Missing expression before {token.val}"));
        }

        return node;
    }

    public AST Expr() {
        AST node = this.Term();

        while (new HashSet<string>{Tokens.PLUS, Tokens.MINUS}.Union(Lexer.CONDITIONALS).Contains(this.current_token.type)) {
            Token token = this.current_token;
            Type ast = null;
            if (token.type == Tokens.PLUS) {
                this.Eat(Tokens.PLUS);
                ast = typeof(Sum);
            }
            else if (token.type == Tokens.MINUS) {
                this.Eat(Tokens.MINUS);
                ast = typeof(Substraction);
            }
            else if (token.type == Tokens.EQUALS) {
                this.Eat(Tokens.EQUALS);
                ast = typeof(Equals);
            }
            else if (token.type == Tokens.HIGHER) {
                this.Eat(Tokens.HIGHER);
                ast = typeof(Higher);
            }
            else if (token.type == Tokens.LOWER) {
                this.Eat(Tokens.LOWER);
                ast = typeof(Lower);
            }
            else {
                this.Error(new SyntaxError($"Unknown operand: {token.type}"));
            }

            node = (AST) Activator.CreateInstance(ast, args: new Object[] {node, this.Factor()});
        }
        return node;
    }

    public int Guess() {
        // give an (educated) guess of what kind of tokens are coming next and how they should be parsed

        var token = this.current_token.type;
        // 2 <=> declaration
        // if there are not other tokens but these two
        int count_ids = 0;
        int _let = 0;
        int others = 0;
        int counter = 0;

        while (token != Tokens.END && token != Tokens.EOF) {
            if (token == Tokens.ID) {
                count_ids += 1;
            }
            else if (token == Tokens.ASSIGN) {
                // could be let
                if (_let == 0) {
                    return BestGuess.Declaration;
                }
                // let in the left side <=> expr
                return BestGuess.Expression;
            }
            else if (token == Tokens.LET) {
                _let += 1;
            }
            else {
                others += 1;
            }

            token = this.Peek(counter++).type;
        }

        if (count_ids == 2 && others == 0) {
            return BestGuess.Declaration;
        }
        return BestGuess.Expression;
    }

    protected AST ParseNode() {
        AST node = null;

        int guessed = Guess();

        if (guessed == BestGuess.Expression) {
            node = this.Expr();
        }
        else if (guessed == BestGuess.Declaration) {
            node = this.Declaration();
        }

        if (this.current_token.type != Tokens.END) {
            this.Error(new SyntaxError("Expected ';'"));
        }

        if (!(node is null)) {
            Exception? exc = node.Check();
            if (!(exc is null)) {
                this.Error(exc);
            }
        }

        return node;
    }

    public AST Parse() {
        List<AST> nodes = new List<AST>();
        AST node = null;

        while (this.current_token.type != Tokens.EOF) {
            node = this.ParseNode();
            this.Eat(Tokens.END);
            nodes.Add(node);
        }

        return new BlockNode(nodes);
    }
}
