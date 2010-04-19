using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

// Handling json in C#
//
// Newtonsoft.Json expects a matching namespace/class heirarchy to serialize
// and deserialise objects to and from a json service.
//
// In this example, Incite.Support is the base for User and Token classes.
// The types declared in each are simple primitives.
//
// JSONRequest handles HTTP actions - GET, POST, PUT, DELETE.
//
// See the Demo class at the bottom of the file for an example.

namespace Incite {
    namespace Support {
        public class PartnerRequest {
            public static string base_uri = "http://10.212.22.11:3000";
            public static string partner_key = "";
        }

        public class User {
            public Response response;
            public string[] messages;

            public class Response {
                public User user;

                public class User {
                    public DateTime created_at, token_expires_at, remember_token_expires_at;
                    public string salt, telephone, member_name, last_name, email, crypted_password, updated_at, token, remember_token, first_name;
                    public int portal_id, id;
                }
            }

            public static string api_uri = "{0}/users.json?partner_key={1}";

            public static User Create(string json_request) {
                return JSONRequest<User>.Post(string.Format(api_uri, PartnerRequest.base_uri, PartnerRequest.partner_key), json_request);
            }

            public class Token : PartnerRequest {
                public Response response;

                public class Response {
                    public DateTime token_expires_at;
                    public string token;
                }

                public static string api_uri = "{0}/users/{1}/token.json?partner_key={2}";

                public static Token Create(int portal_id) {
                    return JSONRequest<Token>.Post(string.Format(api_uri, PartnerRequest.base_uri, portal_id.ToString(), PartnerRequest.partner_key), "[]");
                }
            }
        }
    }
}

public class JSONRequest<T> {
    public static T Get(string uri, string args) {
        return Request(uri, args, "GET");
    }

    public static T Post(string uri, string args) {
        return Request(uri, args, "POST");
    }

    public static T Put(string uri, string args) {
        return Request(uri, args, "PUT");
    }

    public static T Delete(string uri, string args) {
        return Request(uri, args, "DELETE");
    }

    private static T Request(string uri, string args, string method) {
        WebRequest request = WebRequest.Create(uri);

        if (method == "POST" || method == "PUT") {
            var encoding = Encoding.UTF8;
            byte[] data = encoding.GetBytes(args);

            request.Method = method;
            request.ContentType = "application/json";
            request.ContentLength = data.Length;

            Stream post_params = request.GetRequestStream();
            post_params.Write(data, 0, data.Length);
            post_params.Close();
        }

        HttpWebResponse response = null;
        // ignore non-200 errors
        try {
            response = (HttpWebResponse) request.GetResponse();
        } catch(WebException e) {
            response = (HttpWebResponse) e.Response;
        }

        Stream file = response.GetResponseStream();
        StreamReader fp = new StreamReader(file);
        string jsonText = fp.ReadToEnd();

        T result = (T) JsonConvert.DeserializeObject(jsonText, typeof(T));
        Console.WriteLine(result);
        return result;
    }
}

public class TrustAllCertificatePolicy : System.Net.ICertificatePolicy {
    public TrustAllCertificatePolicy() { }

    public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest req, int problem) {
        return true;
    }
}

public class Demo {
    public static int Main(string[] args) {
        System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();

        if (args.Length != 1) {
            Console.Error.WriteLine("usage: InciteSupportAPI.exe portal_id");
            Console.Error.WriteLine("\tcreates auth token for portal_id, then creates a user for the hell of it");
            return 1;
        }

        var request = new { user = new { first_name = "fartyphuckborlz", company_id = 1, portal_id = 223482 } };
        string json_request = JsonConvert.SerializeObject(request);
        var user = Incite.Support.User.Create(json_request);

        return 0;

    }
}

