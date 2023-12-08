namespace Interpreter;

public static class EvalHandler {

    public static Dictionary<string, object> Eval(string text) {
        Lexer lexer = null;
        lexer = new Lexer(text);

        // string, printable
        Dictionary<string, object> response = new Dictionary<string, object> {
            {"errors", null},
            {"console_log", ""},
            // no errors
            {"success", true}
        };

        if (lexer.LastError.Item1 is not null) {
            response["sucess"] = false;
            response["errors"] = lexer.LastError;
            return response;
        }
        Parser parser = new Parser(lexer);
        if (parser.LastError.Item1 is not null) {
            response["sucess"] = false;
            response["errors"] = parser.LastError;
            return response;
        }
        // explicit
        response["errors"] = null;
        response["success"] = true;
        response["console_log"] = new _Interpreter(parser).Interpret();

        return response;
    }
}
