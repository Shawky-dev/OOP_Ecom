﻿using server;
using System.Net;
using System.Text;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

class Program
{

    static void Main(string[] args)
    {
        //database object to hold the txt file paths
        databasePaths Paths = new databasePaths();

        Paths.ItemPath = @"databaseFolder\items\items.txt";
        Paths.CustomerPath = @"databaseFolder\customers\customers.txt";
        Paths.AdminPath = @"databaseFolder\admins\admins.txt";

        //start up server
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("Listening...");
        //intialize router and adding PATHS with their method types (GET,POST,Etc.)for incoming requets




        Router router = new Router();
        //router ITEMS paths
        router.AddRoute("/items", "GET", (context, parameters, requestBody) =>
        {
            return FileOperations.GetAllObjects<Item>(Paths.ItemPath);
        });

        router.AddRoute("/items/{id}", "GET", (context, parameters, requestBody) =>
        {
            int id = Int32.Parse(parameters["id"]);
            //use item id to GET
            return FileOperations.GetObjectByID<Item>(id, Paths.ItemPath);

        });

        router.AddRoute("/items/{id}", "DELETE", (context, parameters, requestBody) =>
        {
            int id = Int32.Parse(parameters["id"]);
            //use item id to GET
            FileOperations.DeleteObjectByID<Item>(id, Paths.ItemPath);
            return new { message = $"Item with id:{id} has been deleted" };
        });

        router.AddRoute("/items/{id}", "PUT", (context, parameters, requestBody) =>
        {
            int id = Int32.Parse(parameters["id"]);

            var newobject = JsonConvert.DeserializeObject<Item>(requestBody);
            FileOperations.ChangeObjectByID<Item>(id, newobject, Paths.ItemPath);

            return new { message = $"Item has been changed" };
        });
        router.AddRoute("/newitem", "POST", (context, parameters, requestBody) =>
        {
            var allobjects = FileOperations.GetAllObjects<Item>(Paths.ItemPath);

            var newobject = JsonConvert.DeserializeObject<Item>(requestBody);
            var id = IDGenerator.GenerateUniqueID<Item>(allobjects);
            newobject.ID = id;

            FileOperations.AddObjectToFile<Item>(newobject, Paths.ItemPath);
            return new { message = $"Item has been added" };
        });


        router.AddRoute("/search/{id}", "POST", (context, parameters, requestBody) =>
        {
            var id = parameters["id"];
            JObject jsonObject = JObject.Parse(requestBody);
            string method = jsonObject["method"].ToString();

            if(method == "search")
            if (id != "*")
            {
                return FileOperations.GetAllObjectsFromSearch<Item>(id, Paths.ItemPath);
            }
            else
            {

                return FileOperations.GetAllObjects<Item>(Paths.ItemPath);
            }else if(method == "category")
            {
                if (id != "*")
                {
                    return FileOperations.GetAllObjectsFromCategory<Item>(id, Paths.ItemPath);
                }
                else
                {
                    return FileOperations.GetAllObjects<Item>(Paths.ItemPath);
                }
            }
            return new { message =id };
        });



        //Router customers
        router.AddRoute("/customers", "GET", (context, parameters, requestBody) =>
        {
            return FileOperations.GetAllObjects<customer>(Paths.CustomerPath);
        });
        router.AddRoute("/newCustomer", "POST", (context, parameters, requestBody) =>
        {
            var newobject = JsonConvert.DeserializeObject<customer>(requestBody);
            if (FileOperations.GetUserByEmail<customer>(newobject._mail, Paths.CustomerPath) == null)
            {
                var allobjects = FileOperations.GetAllObjects<customer>(Paths.CustomerPath);
                var id = IDGenerator.GenerateUniqueID<customer>(allobjects);
                newobject.ID = id;
                FileOperations.AddObjectToFile<customer>(newobject, Paths.CustomerPath);
                return new
                {
                    message = $"user has been added",
                    user = newobject,
                    code = 200
                };
            }
            else
            {
                return new
                {
                    message = "email already exists",
                    code = 409
                };
            }


        });
        router.AddRoute("/customers/{id}", "GET", (context, parameters, requestBody) =>
        {
            int id = Int32.Parse(parameters["id"]);
            //use item id to GET
            return FileOperations.GetObjectByID<customer>(id, Paths.CustomerPath);

        });
        router.AddRoute("/customers/{id}", "DELETE", (context, parameters, requestBody) =>
        {
            int id = Int32.Parse(parameters["id"]);
            //use item id to GET
            FileOperations.DeleteObjectByID<customer>(id, Paths.CustomerPath);
            return new { message = $"Item with id:{id} has been deleted" };
        });

        router.AddRoute("/customers/{id}", "PUT", (context, parameters, requestBody) =>
        {
            int id = Int32.Parse(parameters["id"]);
            var newobject = JsonConvert.DeserializeObject<customer>(requestBody);
            var oldobject = FileOperations.GetObjectByID<customer>(id, Paths.CustomerPath);
            oldobject.Name = newobject.Name;
            oldobject.Address = newobject.Address;
            oldobject._mail = newobject._mail;
            oldobject._pass = newobject._pass;
            oldobject.ProfilePictureBase64 = newobject.ProfilePictureBase64;


            FileOperations.ChangeObjectByID<customer>(id, newobject, Paths.CustomerPath);

            return new { message = $"Item has been changed" };
        });
        router.AddRoute("/authCustomer", "POST", (context, parameters, requestBody) =>
        {

            JObject jsonObject = JObject.Parse(requestBody);
            string email = jsonObject["_mail"].ToString();
            string password = jsonObject["_pass"].ToString();

            var customer = FileOperations.GetUserByEmail<customer>(email, Paths.CustomerPath);

            return FileOperations.CheckUserPassword<customer>(customer, password);
        });


        //Carts
        router.AddRoute("/cart/{id}", "GET", (context, parameters, requestBody) =>
        {
            int id = Int32.Parse(parameters["id"]);
            var customer = FileOperations.GetObjectByID<customer>(id, Paths.CustomerPath);

            return customer.userCart;
        });


        router.AddRoute("/cart/{id}", "PUT", (context, parameters, requestBody) =>
        {
            JObject jsonObject = JObject.Parse(requestBody);

            string method = jsonObject["method"].ToString();
            int itemid = Int32.Parse(parameters["id"]);
            int userID = Int32.Parse(jsonObject["userID"].ToString());

            var customer = FileOperations.GetObjectByID<customer>(userID, Paths.CustomerPath);

            var item = FileOperations.GetObjectByID<Item>(itemid, Paths.ItemPath);
            if (customer != null)
            {

                if (method == "add")
                {
                    customer.addItem(item);
                    FileOperations.ChangeObjectByID<customer>(userID, customer, Paths.CustomerPath);

                    return new
                    {
                        message = "added item"
                    };
                }
                else if (method == "remove")
                {
                    customer.removeItem(item);
                    FileOperations.ChangeObjectByID<customer>(userID, customer, Paths.CustomerPath);
                    return new
                    {
                        message = "removed item"
                    };
                }
            }

            return new
            {
                message = "err"
            };
        });
        router.AddRoute("/cartCheckout/{id}", "POST", (context, parameters, requestBody) =>
        {
            int id = Int32.Parse(parameters["id"]);
            var customer = FileOperations.GetObjectByID<customer>(id, Paths.CustomerPath);
            customer.AddHistroy();
            FileOperations.ChangeObjectByID<customer>(id,customer, Paths.CustomerPath);
            return customer.userHistory;
        });

        //AdminRoutes__________
        router.AddRoute("/admins", "GET", (context, parameters, requestBody) =>
        {
            return FileOperations.GetAllObjects<admin>(Paths.AdminPath);
        });

        router.AddRoute("/newAdmin", "POST", (context, parameters, requestBody) =>
        {
            var newobject = JsonConvert.DeserializeObject<admin>(requestBody);
            if (FileOperations.GetUserByEmail<admin>(newobject._mail, Paths.AdminPath) == null)
            {
                var allobjects = FileOperations.GetAllObjects<admin>(Paths.AdminPath);
                var id = IDGenerator.GenerateUniqueID<admin>(allobjects);
                newobject.ID = id;
                FileOperations.AddObjectToFile<admin>(newobject, Paths.AdminPath);
                return new
                {
                    message = $"user has been added",
                    user = newobject,
                    code = 200
                };
            }
            else
            {
                return new
                {
                    message = "email already exists",
                    code = 409
                };
            }


        });

        router.AddRoute("/admins/{id}", "GET", (context, parameters, requestBody) =>
        {

            int id = Int32.Parse(parameters["id"]);
            //use item id to GET
            return FileOperations.GetObjectByID<admin>(id, Paths.AdminPath);

        });
        router.AddRoute("/admins/{id}", "DELETE", (context, parameters, requestBody) =>
        {
            int id = Int32.Parse(parameters["id"]);
            //use item id to GET
            FileOperations.DeleteObjectByID<admin>(id, Paths.AdminPath);
            return new { message = $"Item with id:{id} has been deleted" };
        });

        router.AddRoute("/admins/{id}", "PUT", (context, parameters, requestBody) =>
        {
            int id = Int32.Parse(parameters["id"]);

            var newobject = JsonConvert.DeserializeObject<admin>(requestBody);
            FileOperations.ChangeObjectByID<admin>(id, newobject, Paths.AdminPath);

            return new { message = $"Item has been changed" };
        });
        router.AddRoute("/authAdmin", "POST", (context, parameters, requestBody) =>
        {

            JObject jsonObject = JObject.Parse(requestBody);
            string email = jsonObject["_mail"].ToString();
            string password = jsonObject["_pass"].ToString();

            var admin = FileOperations.GetUserByEmail<admin>(email, Paths.AdminPath);

            return FileOperations.CheckUserPassword<admin>(admin, password);
        });




        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string responseString = router.ProcessRequest(context);

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;

            // Add CORS headers to allow cross-origin requests
            response.AddHeader("Access-Control-Allow-Origin", "*"); // Allow any origin
            response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS,PUT,DELETE"); // Allow specific methods
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type"); // Allow specific headers

            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }
}
