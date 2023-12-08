namespace Interpreter;

public static class EvalHandler {

    public static Dictionary<string, object> Eval(string text) {
        Dictionary<string, object> response = new Dictionary<string, object> {
            {"errors", null},
            {"console_log", ""},
            // no errors
            {"success", true}
        };
        Lexer lexer = null;
        try {
            lexer = new Lexer(text);
        }
        catch (Exception e) {
            response["success"] = false;
            response["errors"] = e;
            return response;       
        }

        // string, printable

        if (lexer.LastError.Item1 is not null) {
            response["success"] = false;
            response["errors"] = lexer.LastError;
            return response;
        }
        Parser parser;

        try {
            parser = new Parser(lexer);
        }
        catch (Exception e) {
            response["success"] = false;
            response["errors"] = e;
            return response;
        }
        parser = new Parser(lexer);
        if (parser.LastError.Item1 is not null) {
            response["success"] = false;
            response["errors"] = parser.LastError;
            return response;
        }
        // explicit
        response["errors"] = null;
        response["success"] = true;
        response["console_log"] = "";
        try {
            response["console_log"] = new _Interpreter(parser).Interpret();
        }
        catch (Exception e) {
            response["success"] = false;
            response["errors"] = e;
            return response;
        }

        return response;
    }
}
