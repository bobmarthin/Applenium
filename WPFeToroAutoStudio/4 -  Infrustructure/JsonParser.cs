﻿using System;
using System.Configuration;
using System.Data;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Applenium
{
    internal class JsonParser
    {
        public string ReadJson(string parameterName)
        {
            if (parameterName == null) throw new ArgumentNullException("parameterName");
            string text = File.ReadAllText(ConfigurationManager.AppSettings["ConfigurationJsonFile"]);
            JObject json = JObject.Parse(text);
            var parameterValue = (string) json[parameterName];
            return parameterValue;
        }

        public DataTable ReadJsonToDt()
        {
            var dt = new DataTable();
            dt.Columns.Add("Name");
            dt.Columns.Add("Value");

            string text = File.ReadAllText(ConfigurationManager.AppSettings["ConfigurationJsonFile"]);
            JObject json = JObject.Parse(text);
            foreach (var jv in json)
            {
                var data = new[] {jv.Key, jv.Value.ToString()};
                dt.Rows.Add(data);
            }

            ////sort 
            //DataView dv = dt.DefaultView;
            //dv.Sort = "Name";
            //DataTable sortedDT = dv.ToTable();
            return dt;
        }

        public void WriteJson(string name, string value)
        {
            if (value == null) throw new ArgumentNullException("value");
            string text = File.ReadAllText(ConfigurationManager.AppSettings["ConfigurationJsonFile"]);
            JObject json = JObject.Parse(text);
            json[name] = value;
            File.WriteAllText(ConfigurationManager.AppSettings["ConfigurationJsonFile"], json.ToString());
        }

       

    }
}