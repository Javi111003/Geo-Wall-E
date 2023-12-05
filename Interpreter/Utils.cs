
namespace Interpreter;

public class Context : Dictionary<string, dynamic> {

    public Context(dynamic ls) {
        foreach(var declaration in ls) {
            this[declaration.name] = declaration;
        }
    }

    public Context() : base() {}

    public dynamic this[string key] {
        get {
            try {
                return base[key];
            }
            catch(System.Collections.Generic.KeyNotFoundException) {
                throw new NameError(key + " is not defined");
            }
        }
        set {
            base[key] = value;
        }
    }

    public Context Clone() {
        Context ret = new Context();
        foreach(KeyValuePair<string, dynamic> entry in this) {
            ret.Add(entry.Key, (dynamic) entry.Value);
        }
        return ret;
    }
}

public static class BestGuess {
    public static int Expression = 1;
    public static int Declaration = 2;
    public static int BuiltinSugar = 3;
}

