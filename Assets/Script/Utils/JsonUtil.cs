using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class JsonUtil {
    public static Loops LoadColorFromFile()
    {
        string json = "{\"ballz\":[{\"color\":[255,153,0],\"minScore\":1,\"maxScore\":4},{\"color\":[255,224,0],\"minScore\":5,\"maxScore\":15},{\"color\":[168,224,57],\"minScore\":16,\"maxScore\":30},{\"color\":[234,34,94],\"minScore\":31,\"maxScore\":49},{\"color\":[102,0,153],\"minScore\":50,\"maxScore\":69},{\"color\":[0,51,204],\"minScore\":70,\"maxScore\":100},{\"color\":[22,116,188],\"minScore\":101,\"maxScore\":300},{\"color\":[0,153,139],\"minScore\":301,\"maxScore\":500},{\"color\":[0,108,0],\"minScore\":501,\"maxScore\":800},{\"color\":[153,51,0],\"minScore\":801,\"maxScore\":1200},{\"color\":[96,0,0],\"minScore\":1201,\"maxScore\":1800},{\"color\":[34,49,114],\"minScore\":1801,\"maxScore\":3000},{\"color\":[255,153,0],\"minScore\":3001,\"maxScore\":5000},{\"color\":[255,224,0],\"minScore\":5001,\"maxScore\":8000}]}";

        if (json.Length > 0)
        {
            return JsonUtility.FromJson<Loops>(json);
        }

        return null;
    }
}
