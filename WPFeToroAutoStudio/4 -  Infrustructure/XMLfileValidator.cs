using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Applenium._3___DAL;
using Applenium._3___DAL.DataSetAutoTestTableAdapters;
using System.Data;



namespace Applenium
{
    class _XMLfileValidator
    {
        private string _file;
        private string _file_name;
        private string _dir_name;
        private string _file_name_prefix;
        private int _pid;
        private string _pprefix;
        private int _ppid;
        private string _VersionName;
        private Boolean _dbInsertEnabled;
        private string _log_file;


        public Hashtable DS = new Hashtable(); //create directories structure hashtable

        public _XMLfileValidator(string filename, Boolean MobileProjectEnabled, string MOPid, string MOPprefix, string MOPpageID, string testingEnvironmentVersionColumn, string log_dir)
        {
            _file = filename.TrimEnd();
            _dbInsertEnabled = MobileProjectEnabled;
            _VersionName = testingEnvironmentVersionColumn;


            FileInfo fi = new FileInfo(_file);

            _file_name = fi.Name;
            _dir_name = fi.DirectoryName;
            _log_file = log_dir + @"\applenium_xml_indexation.log";

            Regex rfn = new Regex(@"^\s*(?<prefix>\w+)\.(?<suffix>\w+)\s*$", RegexOptions.Multiline);

            Match mfn = rfn.Match(_file_name);

            if (mfn.Success)
            {
                _file_name_prefix = mfn.Result("${prefix}_${suffix}");
            }
            else
            {
                _file_name_prefix = _file_name.Replace(@".", @"_");
            }

            if (MobileProjectEnabled)
            {
                _pid = Convert.ToInt32(MOPid);
                _pprefix = MOPprefix.Trim();
                _ppid = Convert.ToInt32(MOPpageID);

            }
            else
            {
                _pid = 0;
                _pprefix = "";
                _ppid = 0;

            }

        }

        public Boolean check_file()
        {
            // Check that XML file is exists
            if (!System.IO.File.Exists(_file))
            {
                return false;
            }
            return true;
        }


        public List<string> xml_element_id_validator(Boolean backup)
        {
            var i = 0;
            var y = 0;
            var c = 0;
            var x = 0;
            List<string> out_stat = new List<string>();



            if (backup)
            {
                string backup_file = _dir_name + @"\__" + _file_name + @".back";

                if (File.Exists(backup_file)) File.Delete(backup_file);

                File.Copy(this._file, backup_file);
            }

            //Hashtable file_map = new Hashtable();
            SortedDictionary<int, int> file_map = new SortedDictionary<int, int>();
            DS.Add(this._file, file_map);
            var str_time = DateTime.Now.ToString();

            FileStream f = new FileStream(_log_file, FileMode.Append, FileAccess.Write);
            StreamWriter log_out = new StreamWriter(f);

            XNamespace android = "http://schemas.android.com/apk/res/android";
            XNamespace app = "http://schemas.android.com/apk/res-auto";
            XDocument xDoc = XDocument.Load(_file, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);//, LoadOptions.PreserveWhitespace


            log_out.WriteLine("Start: " + this._file + "(Time:" + str_time + ")");
            log_out.WriteLine("-----------------------------------");
            log_out.WriteLine("Elements without ID:\r\n");

            out_stat.Add("Start: " + this._file + "(Time:" + str_time + ")\r\n");
            out_stat.Add("-----------------------------------\r\n");
            out_stat.Add("Elements without ID:\r\n");

            //XComment //XElement //XText
            foreach (var tmpel in xDoc.Root.DescendantNodesAndSelf())
            {
                if (!(tmpel is XElement))
                {
                    continue;
                }

                XElement el = (XElement)tmpel;

                c++;

                XAttribute id = el.Attributes().FirstOrDefault(a => a.Name.LocalName == "id");
                if (id == null)
                {
                    i++;
                    log_out.WriteLine(el.Name + "line: " + ((IXmlLineInfo)el).LineNumber + "");
                    out_stat.Add(el.Name + "line: " + ((IXmlLineInfo)el).LineNumber + "\r\n");
                }
                else
                {
                    y++;

                    XAttribute contentDescription = el.Attributes().FirstOrDefault(a => a.Name.LocalName == "contentDescription");

                    if (contentDescription != null)
                    {
                        int tmp = ((IXmlLineInfo)contentDescription).LineNumber;

                        file_map.Add(x++, (tmp * -1));

                    }
                    else
                    {
                        file_map.Add(x++, ((IXmlLineInfo)id).LineNumber);
                    }
                }

            }
            log_out.WriteLine("-----------------------------------");
            out_stat.Add("-----------------------------------\r\n");

            List<string> txtLines = new List<string>();

            int ln = 0;
            int pageSectionID = 0;
            int addLineCount = 1;
            string edit_mode = "";
            string ElementName = "";

            Regex id_chech_line_str = new Regex(@":id=");

            //<ImageView android:id="@id/abs__up"
            //    <ImageView android:id="@id/abs__up"
            //In case ID is first attribute included "<" elements
            Regex rFirstLine = new Regex(@"^(?<FirstLineElementName>\s*\<\w+\s+)(?<start_line>\w+\:).*?/(?<id_name>\w+)", RegexOptions.IgnoreCase);

            Regex r = new Regex(@"^(?<start_line>\s+\w+\:).*?/(?<id_name>\w+)", RegexOptions.IgnoreCase);   //_icon   |_image


            //Regex rElementsFilter = new Regex(@"(_divivder|_divider|_separtor|_separator|_container|_frame|_area)"); // maybe with \s*$ 
            Regex rElementsFilter = new Regex(@"(_divivder|_divider|_separtor|_separator)"); // maybe with \s*$

            Regex CD_check_line = new Regex(@"contentDescription=");
            Regex CD_get_str = new Regex("contentDescription=\"(?<descriptorname>.*?)\"");


            log_out.WriteLine("CD (contentDescription) in work:");

            out_stat.Add("CD (contentDescription) in work:\r\n");

            foreach (string str in File.ReadAllLines(_file))
            {
                Match id_str = id_chech_line_str.Match(str);
                Match CDreg = CD_get_str.Match(str);
                Boolean firstLineID = false;


                if (file_map.ContainsValue((ln + 1) * -1) && (CDreg.Success))
                {
                    edit_mode = "update";
                    ElementName = CDreg.Result("${descriptorname}");
                    log_out.WriteLine("CD update " + ElementName + " (line:" + (ln + 1) + ")");
                    out_stat.Add("CD update " + ElementName + " (line:" + (ln + 1) + ")\r\n");

                }
                else if (file_map.ContainsValue(ln + 1) && (id_str.Success))
                {

                    edit_mode = "new";
                    string new_str = "";

                    Match m = r.Match(str);
                    Match mFirstLine = rFirstLine.Match(str);
                    if (m.Success)
                    {

                        ElementName = this._file_name_prefix + @"__" + m.Result("${id_name}");

                        new_str = m.Result("${start_line}") + "contentDescription=\"" + ElementName + "\"";

                        txtLines.Add(new_str);


                        log_out.WriteLine("CD added to element" + ElementName + " (line:" + (ln + addLineCount) + ")");

                        out_stat.Add("CD added to element" + ElementName + " (line:" + (ln + addLineCount) + ")\r\n");
                        addLineCount++;
                    }
                    else if (mFirstLine.Success)
                    {
                        ElementName = this._file_name_prefix + @"__" + mFirstLine.Result("${id_name}");

                        firstLineID = true;

                        string strL = mFirstLine.Result("${FirstLineElementName}");

                        string strPattern = @".";

                        string replacement = " ";

                        Regex startSpace = new Regex(strPattern);

                        string Spaceresult = startSpace.Replace(strL, replacement);

                        new_str = Spaceresult + mFirstLine.Result("${start_line}") + "contentDescription=\"" + ElementName + "\"";

                        txtLines.Add(str);
                        txtLines.Add(new_str);

                        log_out.WriteLine("CD added to element" + ElementName + " (line:" + (ln + addLineCount + 1) + ")");
                        out_stat.Add("CD added to element" + ElementName + " (line:" + (ln + addLineCount + 1) + ")\r\n");
                        addLineCount++;
                    }

                }
                if (_dbInsertEnabled)
                {
                    if (!(String.IsNullOrEmpty(edit_mode)))
                    {
                        if (!(String.IsNullOrEmpty(ElementName)))
                        {
                            Match mElementsFilter = rElementsFilter.Match(ElementName);
                            if (!(mElementsFilter.Success))
                            {
                                string page_name = this._pprefix + this._file_name_prefix;

                                pageSectionID = SilenceCreateNewPageSection(page_name, this._ppid);
                                SilenceCreateNewElement(pageSectionID, ElementName);
                            }
                        }
                    }

                }

                ln++;
                if (!firstLineID)
                {
                    txtLines.Add(str);
                }
                edit_mode = "";
                ElementName = "";
            }

            FileStream xml_stream = new FileStream(this._file, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter xout = new StreamWriter(xml_stream);

            foreach (string str in txtLines)
            {
                xout.WriteLine(str);
            }
            xout.Close();


            log_out.WriteLine("------------------------------------------------------------------------------------------------------------");
            out_stat.Add("------------------------------------------------------------------------------------------------------------\r\n");


            log_out.WriteLine("\r\nElements summary: All: " + c + ", Elements with id=" + y + ", Elements without id=" + i);
            log_out.WriteLine("Finish: " + this._file + "\r\n------------------------------------------------------------------------------------------------------------\r\n");

            out_stat.Add("\r\nElements summary: All: " + c + ", Elements with id=" + y + ", Elements without id=" + i + "\r\n");
            out_stat.Add("Finish: " + this._file + "\r\n------------------------------------------------------------------------------------------------------------\r\n");

            log_out.Close();

            return out_stat;
        }

        private int SilenceCreateNewPageSection(string pageSectionName, int pageID)
        {
            int pageSectionID = 0;
            if (pageSectionName == string.Empty)
            {
                return pageSectionID;
            }

            var sql = new Sql();
            var dataset = new DataSetAutoTest();
            var adapterPageSection = new GuiPageSectionTableAdapter();
            Boolean flag = true;

            adapterPageSection.Fill(dataset.GuiPageSection);

            DataTableReader dtReader = dataset.GuiPageSection.CreateDataReader();

            while (dtReader.Read())
            {
                var pID = dtReader.GetValue(1).ToString().Trim();
                var pName = dtReader.GetValue(2).ToString().Trim();
                if (pName == pageSectionName && Convert.ToInt32(pID) == pageID)
                {
                    flag = false;

                    // get existing page section ID
                    pageSectionID = Convert.ToInt32(dtReader.GetValue(0).ToString().Trim());
                    break;
                }

            }
            dtReader.Close();

            if (!flag)
            {
                return pageSectionID;
            }
            // create new page section
            adapterPageSection.Insert(pageID, pageSectionName);

            // Get new Page Section's ID
            adapterPageSection.Fill(dataset.GuiPageSection);

            dtReader = dataset.GuiPageSection.CreateDataReader();
            while (dtReader.Read())
            {
                var pID = dtReader.GetValue(1).ToString().Trim();
                var pName = dtReader.GetValue(2).ToString().Trim();

                if (pName == pageSectionName && Convert.ToInt32(pID) == pageID)
                {
                    pageSectionID = Convert.ToInt32(dtReader.GetValue(0).ToString().Trim());
                    break;
                }
            }
            dtReader.Close();
            return pageSectionID;

        }

        private void SilenceCreateNewElement(int pageSectionID, string ElementName)
        {

            Boolean flag = true;
            var sql = new Sql();
            var dataset = new DataSetAutoTest();
            var adapterGuimap = new GuiMapTableAdapter();

            if (pageSectionID > 0)
            {

                adapterGuimap.Fill(dataset.GuiMap);
                DataTableReader dtElementReader = dataset.GuiMap.CreateDataReader();

                while (dtElementReader.Read())
                {
                    //GuiMapID 0
                    var col1 = dtElementReader.GetValue(1).ToString().Trim(); //GuiMapObjectName
                    var col2 = dtElementReader.GetValue(2).ToString().Trim(); //TagTypeID === 2
                    var col3 = dtElementReader.GetValue(3).ToString().Trim(); //TagTypeValue
                    var col4 = Convert.ToInt32(dtElementReader.GetValue(4).ToString().Trim()); //GuiProjectID == PageSection

                    if (col4 == pageSectionID && col3 == ElementName)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    string guiMapObjectName = _pprefix + ElementName;
                    adapterGuimap.Insert(guiMapObjectName, 2, ElementName, pageSectionID);
                    string GuiMapId = adapterGuimap.GetLastGuiMapId().ToString();
                    sql.UpdateVersion("GuiMap", "GuiMapID", GuiMapId, this._VersionName, 1);
                }

            }

        }

    }
}
