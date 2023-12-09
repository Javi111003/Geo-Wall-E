using System.IO;
using System.Reflection;

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

static class Settings {

    public static DirectoryInfo BASE_DIR = new DirectoryInfo(
        Assembly.GetAssembly(typeof (_Interpreter)).Location
    ).Parent.Parent.Parent.Parent.Parent;

    public static DirectoryInfo PWD = new DirectoryInfo(Directory.GetCurrentDirectory());

    public static DirectoryInfo APP_DIR = new DirectoryInfo(Path.Join(BASE_DIR.ToString(), "Libraries"));

    public static DirectoryInfo[] GSHARPATH = new DirectoryInfo[3]{PWD, APP_DIR, BASE_DIR};
}
