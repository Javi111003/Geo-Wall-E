
namespace Interpreter;

public class Context : Dictionary<string, dynamic> {

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
