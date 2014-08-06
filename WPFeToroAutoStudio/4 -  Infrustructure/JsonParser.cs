using System;
using System.Configuration;
using System.Data;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

using System.Linq;
using System.Collections;
using System.Collections.Generic;


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
                //VSH: update global dictionary
                var key = jv.Key.ToString();
                var value = jv.Value.ToString();
                AddKeyToMemory(key, value);

                var data = new[] { jv.Key, value };
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

        /// <summary>
        /// 
        /// VSH: create / update global memory
        /// configuration  from configuration file  
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Boolean</returns>

        public  Boolean AddConfigToMemory(string file = "" )
        {
            Boolean res = true;
            string text = "";
            Regex rgx = new Regex(@"Configuration.json");

            if (file == "")
            {
                //Read default configuration file
                text = File.ReadAllText(ConfigurationManager.AppSettings["ConfigurationJsonFile"]);

                //Clear existing dictionary in case of default configuration  
                Constants.MemoryConf.Clear();
            }
            else
            {
                //Check if it's default configuration file
                Match matching = rgx.Match(file);
                if (matching.Success)
                {
                    //Clear existing dictionary in case of default configuration  
                    Constants.MemoryConf.Clear();
                }
                //Read additional configuration file - create new or overwrite old values
                text = File.ReadAllText(file);
            }

            JObject json = JObject.Parse(text);
           
            foreach (var jv in json)
            {

                var key = jv.Key.ToString();
                var value = jv.Value.ToString();

                //convert variables
                value = replaceVariable(value);
                
                AddKeyToMemory( key, value);

            }

            return res;
        }
        public Boolean AddKeyToMemory(string key, string value)
        {
            Boolean res = true;
            Regex rgx = new Regex(@"^-+");

            // --- clear comment lines ---
            Match matching = rgx.Match(key);
            if (matching.Success)
            {
                return res;
            }

            try
            {
                if (Constants.MemoryConf.ContainsKey(key))
                {
                    Constants.MemoryConf[key] = value;
                }
                else
                {
                    Constants.MemoryConf.Add(key, value);

                }
            }
            catch (Exception e)
            {
                res = false;
            }
            return res;
        }

        public string ValidationRegex(string str, string mode ="")
        {
            string res = "";
            str = replaceVariable(str);
            string val = Constants.MemoryConf["memory"];

            //val = Regex.Escape(val);
            str = Regex.Escape(str);
            Regex rgx = new Regex(str);
            Match matching = rgx.Match(val);
            string CmDInfo = Constants.MemoryConf["LastCmdInfo"];
            
            
            if (matching.Success)
            {
                res = "OK";
                if (mode.Contains("not"))
                {
                    res = "ERROR: CMD 1080: The regex compare is equal (NOT Verification). Validation failed (Memory:" + val + "RegexString: " + str + ")(CmDInfo=" + CmDInfo+ ")";
                }
            }
            else
            {
                res = "ERROR: CMD 1080: The regex compare is not equal. Validation failed (Memory:" + val + "RegexString: "+ str + ")";
                if (mode.Contains("not"))
                {
                    res = "OK";
                }
            }
            return res;
        }

        public Boolean ValidationLineCompare(string str)
        {
            Boolean res = false;
            str = replaceVariable(str);
            string val = Constants.MemoryConf["memory"];
            string[] lines = Regex.Split(val, "\n");

            foreach (string line in lines)
            {
                if (line.ToString().Equals(str))
                {
                    return true;
                }
            }
            return res;
        }

        public String replaceVariable(string str)
        {
            string key = "";
            string val = "";
            string KeySeparator = "";

            Regex rgx = new Regex(@"\$\$(?<keyname>[0-9A-Z-_]+)(?<KeySeparator>\s*\~\s*)?", RegexOptions.IgnoreCase);
            
            Match matching = rgx.Match(str);
            while (matching.Success)
            {
                key = matching.Result("${keyname}");
                KeySeparator = matching.Result("${KeySeparator}");
                
                //if (matching.Result("${KeySeparator}") == "~")
                //{
                //    KeySeparator = matching.Result("${KeySeparator}");
                //}

                if (Constants.MemoryConf.ContainsKey(key))
                {
                    val = Constants.MemoryConf[key].ToString().TrimEnd();

                    Regex rr = new Regex(@"\$\$" + key + KeySeparator);
                    str = rr.Replace(str, val);
                    
                    matching = null;
                    key = null;
                    rr = null;
                    KeySeparator = null;

                    matching = rgx.Match(str);
                }
                else
                {
                    return "ERROR: The variable " + key + " is not defined\n";
                }

            }
            return str;
        }
        /// <summary>
        /// Var manipulation function
        /// Syntax: $$Variable ( += -= *= %= ~= /= = ) (values...) 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public Boolean VariableFunctions(string str)
        {
            bool res = true;
            string key = "";
            string mode = "";
            string val = "";
            string ChangeString;
            
            try
            {
               

                Regex rgx = new Regex(@"^\s*\$\$(?<keyname>[0-9A-Z-_]+)\s*(?<mode>\+=|-=|\*=|/=|\~=|\%=|\.=|j=|r=|f=|=)\s*(?<value>.*)\s*$", RegexOptions.IgnoreCase);
                //Regex rgx = new Regex("^\\s*\\$\\$(?<keyname>[0-9A-Z-_]+)\\s*(?<mode>=|\\+|-|\\*|/|\\~|\\%|\\.)=\\s*(?<value>.*)\\s*$");

                Match matching = rgx.Match(str);
                if (matching.Success)
                {
                    key = matching.Result("${keyname}");
                    mode = matching.Result("${mode}");
                    ChangeString = matching.Result("${value}");
                    ChangeString = replaceVariable(ChangeString);

                    if (mode == "=")
                    {
                        AddKeyToMemory(key, ChangeString);
                    }
                    else if (!(mode == @".=" || mode == @"~=" || mode == @"j=")) // NOT String function
                    {
                        val = Constants.MemoryConf[key];

                        if (!Constants.MemoryConf.ContainsKey(key))
                        {
                            return false;
                        }

                        Regex tc = new Regex(@"\-*(\d+|\d+\.\d+|\d+\,\d+)"); //type check
                        Match tm = tc.Match(ChangeString); //type match
                        Match tmv = tc.Match(val); //type match variable

                        if (tm.Success && tmv.Success)
                        {

                            Double val_int = Convert.ToDouble(val);
                            Double ch_int = Convert.ToDouble(ChangeString);

                            if (mode == @"+=")
                            {
                                AddKeyToMemory(key, Convert.ToString(val_int + ch_int));

                            }
                            else if (mode == @"-=")
                            {
                                AddKeyToMemory(key, Convert.ToString(val_int - ch_int));
                            }
                            else if (mode == @"*=")
                            {
                                AddKeyToMemory(key, Convert.ToString(val_int * ch_int));
                            }
                            else if (mode == @"/=")
                            {
                                AddKeyToMemory(key, Convert.ToString(val_int / ch_int));
                            }
                            else if (mode == @"%=")
                            {
                                AddKeyToMemory(key, Convert.ToString(val_int * ch_int / 100 ));
                            }
                            else if (mode == @"f=") // number formater
                            {
                                string format = @"{0:" + ChangeString + @"}";
                                string tmp = String.Format( format , val_int);
                                AddKeyToMemory(key, tmp);
                            }
                            else if (mode == @"r=") // round number
                            {
                                int tmp = Convert.ToInt32(ch_int);
                                val_int = Math.Round(val_int, tmp, MidpointRounding.ToEven);
                                
                                string tmp1 = Convert.ToString(val_int);

                                Match m = Regex.Match(tmp1, @"\.\d{" + (tmp - 1) + @"}\s*$");
                                if (m.Success)
                                {
                                    tmp1 += 0;
                                }
                                AddKeyToMemory(key, tmp1);
                            }
                            


                        }
                        else
                        {
                            //LOG: ONLY NUMBER 
                            return false;
                        }

                    }
                    else if (mode == @"j=")
                    {
                        AddKeyToMemory(key, getJsonValue(ChangeString));
                    }
                    else if (mode == @"js=")
                    {
                        AddKeyToMemory(key, getJsonValue(ChangeString));
                    }
                    else
                    {
                        val = Constants.MemoryConf[key];

                        if (!Constants.MemoryConf.ContainsKey(key))
                        {
                            return false;
                        }
                        if (mode == @".=")
                        {
                            AddKeyToMemory(key, (val + ChangeString));
                        }
                        else if (mode == @"~=")
                        {
                            AddKeyToMemory(key, regularExp(val, ChangeString));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string vlad = ex.Message;
                string ee = vlad + vlad;
            }

             return res;
        }

        public String getJsonValue(string str)
        {
            str = replaceVariable(str);
            string m = "";
            string r = "";
            string m1 = "";

            if (str.Contains("==>"))
            {
                Regex rgx = new Regex(@"^(?<key>.*)==>(?<json>.*)");
                Match matching = rgx.Match(str);
                if (matching.Success)
                {
                    //key str
                    m = matching.Result("${key}");

                    //json data str
                    r = matching.Result("${json}");
                }
            }
            else
            {
                r = replaceVariable(@"$$memory");
                m = str;

            }


            Boolean f = false;
            Boolean arr = false;
            Boolean obj = false;
            JsonTextReader reader = new JsonTextReader(new StringReader(r));
            string jres = "";
            Regex nl = new Regex(@",\s*$");

            string work_mode = "";
            Regex mode_rm = new Regex(@"\.(array|obj)\s*$");
            Regex types = new Regex(@"(Float|double|long|short|int|decimal|char|string|bool|object|null)", RegexOptions.IgnoreCase);


            while (reader.Read())
            {
                if (reader.TokenType.ToString() == "PropertyName" && reader.Value.ToString() == m)
                {
                    f = true;
                    m1 = m;
                    m = "applenium uniq string - only first similarity enabled";
                    continue;

                }

                if (f)
                {
                    if (reader.TokenType.ToString() == "StartArray")
                    {
                        work_mode += ".array";
                        jres += @"[";
                        obj = false;
                        arr = true;

                        continue;
                    }
                    if (reader.TokenType.ToString() == "StartObject")
                    {
                        jres += @"{";
                        work_mode += ".obj";
                        arr = false;
                        obj = true;

                        continue;
                    }
                    if (reader.TokenType.ToString() == "EndArray")
                    {
                        jres = nl.Replace(jres, "");
                        jres += "],";

                        work_mode = mode_rm.Replace(work_mode, "");

                        if (work_mode == String.Empty)
                        {
                            obj = false;
                            arr = false;
                            jres = nl.Replace(jres, "");
                            break;
                        }
                        else
                        {

                            if (work_mode.EndsWith("obj"))
                            {
                                arr = false;
                                obj = true;

                            }
                            else
                            {
                                obj = false;
                                arr = true;
                            }
                            continue;

                        }
                    }
                    if (reader.TokenType.ToString() == "EndObject")
                    {
                        jres = nl.Replace(jres, "");
                        jres += "},";

                        work_mode = mode_rm.Replace(work_mode, "");

                        if (work_mode == String.Empty)
                        {
                            jres = nl.Replace(jres, "");
                            obj = false;
                            arr = false;
                            break;
                        }
                        else
                        {

                            if (work_mode.EndsWith("obj"))
                            {
                                obj = true;

                            }
                            else
                            {
                                arr = true;
                            }
                            continue;

                        }
                    }

                    if (f & arr)
                    {
                        jres += reader.Value.ToString() + ", ";
                    }
                    else if (f & obj)
                    {
                        
                        if ( reader.Value == null)
                        {
                            jres += "\"\", ";
                            continue;
                        }

                        Match obj_types = types.Match(reader.TokenType.ToString());
                        if (!obj_types.Success)  //if (reader.TokenType.ToString() == "PropertyName")
                        {
                            jres += "\"" + reader.Value.ToString() + "\": ";
                        }
                        else
                        {
                            jres += "\"" + reader.Value.ToString() + "\",";
                        }
                    }
                    else if (f)
                    {
                        jres = reader.Value.ToString();
                        break;
                    }
                }

                //else
                //    Console.WriteLine("Token: {0}", reader.TokenType);
            }
            if (jres.StartsWith(@"["))
            {
                jres = "{\"" + m1 + "\":" + jres + @"}";
            }
            return jres;
        }


        public String JsonSort(string str)
        {
            str = replaceVariable(str);
            string m = "";
            string r = "";
            string m1 = "";
            string include_list = "";
            string visible_list = @".*";

            //include:aaa=xxx,,bbb=yy return=ccc,ddd ==> $$kkkk

            Match match = Regex.Match(str, @"include\s*=\s*(.*?)(\s+return=|\s*==>|\s*$)", RegexOptions.IgnoreCase);
            if (! match.Success)
            {
                return ("ERROR: include list not found\n");
            }
            include_list = match.Groups[1].Value;



            match = Regex.Match(str, @"return\s*=\s*(.*?)(\s+include=|\s*==>|\s*$)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                visible_list = match.Groups[1].Value;
            }
            
            if (str.Contains("==>"))
            {
                Regex rgx = new Regex(@"^(?<key>.*)==>(?<json>.*)");
                Match matching = rgx.Match(str);
                if (matching.Success)
                {
                    //key str
                    m = matching.Result("${key}");

                    //json data str
                    r = matching.Result("${json}");
                }
            }
            else
            {
                r = replaceVariable(@"$$memory");
                m = str;

            }

            Boolean f = false;
            Boolean arr = false;
            Boolean obj = false;
            JsonTextReader reader = new JsonTextReader(new StringReader(r));
            string jres = "";
            Regex nl = new Regex(@",\s*$");

            string work_mode = "";
            Regex mode_rm = new Regex(@"\.(array|obj)\s*$");
            Regex types = new Regex(@"(Float|double|long|short|int|decimal|char|string|bool|object|null)", RegexOptions.IgnoreCase);
            Regex include_search = new Regex(include_list, RegexOptions.IgnoreCase);
            Regex return_search = new Regex(visible_list, RegexOptions.IgnoreCase);


            while (reader.Read())
            {
                if (reader.TokenType.ToString() == "PropertyName" && reader.Value.ToString() == m)
                {
                    f = true;
                    m1 = m;
                    m = "applenium uniq string - only first similarity enabled";
                    continue;

                }

                if (f)
                {
                    if (reader.TokenType.ToString() == "StartArray")
                    {
                        work_mode += ".array";
                        jres += @"[";
                        obj = false;
                        arr = true;

                        continue;
                    }
                    if (reader.TokenType.ToString() == "StartObject")
                    {
                        jres += @"{";
                        work_mode += ".obj";
                        arr = false;
                        obj = true;

                        continue;
                    }
                    if (reader.TokenType.ToString() == "EndArray")
                    {
                        jres = nl.Replace(jres, "");
                        jres += "],";

                        work_mode = mode_rm.Replace(work_mode, "");

                        if (work_mode == String.Empty)
                        {
                            obj = false;
                            arr = false;
                            jres = nl.Replace(jres, "");
                            break;
                        }
                        else
                        {

                            if (work_mode.EndsWith("obj"))
                            {
                                arr = false;
                                obj = true;

                            }
                            else
                            {
                                obj = false;
                                arr = true;
                            }
                            continue;

                        }
                    }
                    if (reader.TokenType.ToString() == "EndObject")
                    {
                        jres = nl.Replace(jres, "");
                        jres += "},";

                        work_mode = mode_rm.Replace(work_mode, "");

                        if (work_mode == String.Empty)
                        {
                            jres = nl.Replace(jres, "");
                            obj = false;
                            arr = false;
                            break;
                        }
                        else
                        {

                            if (work_mode.EndsWith("obj"))
                            {
                                obj = true;

                            }
                            else
                            {
                                arr = true;
                            }
                            continue;

                        }
                    }

                    if (f & arr)
                    {
                        jres += reader.Value.ToString() + ", ";
                    }
                    else if (f & obj)
                    {

                        if (reader.Value == null)
                        {
                            jres += "\"\", ";
                            continue;
                        }

                        Match obj_types = types.Match(reader.TokenType.ToString());
                        if (!obj_types.Success)  //if (reader.TokenType.ToString() == "PropertyName")
                        {
                            jres += "\"" + reader.Value.ToString() + "\": ";
                        }
                        else
                        {
                            jres += "\"" + reader.Value.ToString() + "\",";
                        }
                    }
                    else if (f)
                    {
                        jres = reader.Value.ToString();
                        break;
                    }
                }

                //else
                //    Console.WriteLine("Token: {0}", reader.TokenType);
            }
            if (jres.StartsWith(@"["))
            {
                jres = "{\"" + m1 + "\":" + jres + @"}";
            }
            return jres;
        }

        /// <summary>
        /// Internal Help function  - replace var by regex / capture data to capture[i] memory
        /// </summary>
        /// <param name="val"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public String regularExp(string val, string str)
        {
            str = replaceVariable(str);
            val = replaceVariable(val);

            if(str.Contains("==>"))
            {
                //Split command string to match and replace strings
                Regex rgx = new Regex(@"^(?<match>.*)==>(?<replace>.*)");
                Match matching = rgx.Match(str);
                if (matching.Success)
                {
                    //match str
                    string m = matching.Result("${match}");

                    //replace str
                    string r = matching.Result("${replace}");

                    //prepare regex with user 
                    Regex rr = new Regex(m);
                    val = rr.Replace(val, r);
                }
               
            }
            else
            {
                MatchCollection matches = Regex.Matches(val, str, RegexOptions.IgnoreCase);

                // Use foreach loop.
                if (matches.Count > 0)
                {
                    
                    int i = 1;

                    foreach (Match match in matches)
                    {
                        foreach (Capture capture in match.Captures)
                        {
                            AddKeyToMemory(("capture" + i), capture.Value);
                            i++;
                        }
                    }

                }

            }
            return val;
        }
    }
}