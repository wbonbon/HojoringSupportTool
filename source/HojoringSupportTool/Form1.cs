using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace HojoringSupportTool
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            try
            {
                var ext = "";
                LuminaOptions options = new LuminaOptions();
                bool useGitHub = false;
                string githubBaseUrl = "";

                if (radioButton1.Checked)
                {
                    options.DefaultExcelLanguage = Language.English;
                    ext = ".en-US.csv";
                }
                if (radioButton2.Checked)
                {
                    options.DefaultExcelLanguage = Language.Japanese;
                    ext = ".ja-JP.csv";
                }
                if (radioButton3.Checked)
                {
                    options.DefaultExcelLanguage = Language.German;
                    ext = ".de-DE.csv";
                }
                if (radioButton4.Checked)
                {
                    options.DefaultExcelLanguage = Language.French;
                    ext = ".fr-FR.csv";
                }
                if (radioButton5.Checked)
                {
                    options.DefaultExcelLanguage = Language.Korean;
                    ext = ".ko-KR.csv";
                    useGitHub = true;
                    githubBaseUrl = "https://raw.githubusercontent.com/Ra-Workspace/ffxiv-datamining-ko/master/csv/";
                }
                if (radioButton6.Checked)
                {
                    options.DefaultExcelLanguage = Language.ChineseSimplified;
                    ext = ".zh-CN.csv";
                    useGitHub = true;
                    githubBaseUrl = "https://raw.githubusercontent.com/thewakingsands/ffxiv-datamining-cn/master/";
                }
                if (radioButton7.Checked)
                {
                    options.DefaultExcelLanguage = Language.ChineseTraditional;
                    ext = ".zh-CN.csv";
                    useGitHub = true;
                    githubBaseUrl = "https://raw.githubusercontent.com/thewakingsands/ffxiv-datamining-cn/master/";
                }
                if (radioButton8.Checked)
                {
                    options.DefaultExcelLanguage = Language.TraditionalChinese;
                    ext = ".zh-TW.csv";
                    useGitHub = true;
                    githubBaseUrl = "https://raw.githubusercontent.com/thewakingsands/ffxiv-datamining-tc/master/";
                }

                if (useGitHub)
                {
                    await ProcessFromGitHubAsync(githubBaseUrl, ext);
                    MessageBox.Show("finished.");
                }
                else
                {
                    options.LoadMultithreaded = true;

                    var lumina = new Lumina.GameData(GameDataPath.Text, options);


                    string text = "";

                    // AttackType
                    var sheet_at = lumina.GetExcelSheet<Lumina.Excel.Sheets.AttackType>();
                    if (sheet_at != null)
                    {
                        // Action
                        var sheet_a = lumina.GetExcelSheet<Lumina.Excel.Sheets.Action>();
                        if (sheet_a != null)
                        {
                            text = "key,0,1,2\n#,Name,AttackType,Recast<100ms>\nint32,str,int32,int32\n";
                            foreach (var item in sheet_a)
                            {
                                if (item.AttackType.RowId > 0 && item.AttackType.RowId < 9)
                                {
                                    text += item.RowId + ",\"" + item.Name.ToString() + "\"," + sheet_at.GetRow(item.AttackType.RowId).Name.ExtractText() + "," + item.Recast100ms.ToString() + ",\n";
                                }
                                else
                                {
                                    text += item.RowId + ",\"" + item.Name.ToString() + "\",," + item.Recast100ms.ToString() + ",\n";
                                }
                            }
                            File.WriteAllText(@"Action" + ext, text, System.Text.Encoding.UTF8);
                        }
                    }

                    // Status
                    var sheet_s = lumina.GetExcelSheet<Lumina.Excel.Sheets.Status>();
                    if (sheet_s != null)
                    {
                        text = "key,0\n#,Name\nint32,str\n";
                        foreach (var item in sheet_s)
                        {
                            text += item.RowId + ",\"" + item.Name.ToString() + "\"\n";
                        }
                        File.WriteAllText(@"Status" + ext, text, System.Text.Encoding.UTF8);
                    }

                    var sheet_t = lumina.GetExcelSheet<Lumina.Excel.Sheets.TerritoryType>();
                    // TerritoryType
                    if (sheet_t != null)
                    {
                        text = "key,0,1,2\n#,TerritoryIntendedUse,Resident,ContentFinderCondition\nint32,TerritoryIntendedUse,uint16,ContentFinderCondition\n";
                        foreach (var item in sheet_t)
                        {
                            try
                            {
                                ContentFinderCondition cfc = item.ContentFinderCondition.Value;
                                RowRef<TerritoryType> territory = cfc.TerritoryType;
                                RowRef<TerritoryIntendedUse> intendedUse = item.TerritoryIntendedUse;

                                if (territory.IsValid && !cfc.Name.IsEmpty)
                                {
                                    text += item.RowId + ",\"" + intendedUse.RowId + "\",\"" + item.Resident + "\",\"" + cfc.Name.ToString() + "\"\n";
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                            }
                        }
                        File.WriteAllText(@"TerritoryType" + ext, text, System.Text.Encoding.UTF8);
                    }

                    MessageBox.Show("finished.");
                }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            button1.Enabled = true;
        }

        private async Task ProcessFromGitHubAsync(string baseUrl, string ext)
        {
            // AttackType Dictionary
            var attackTypes = new Dictionary<int, string>();
            var atData = await DownloadCsvLinesAsync(baseUrl + "AttackType.csv");
            int atIdCol = -1, atNameCol = -1;
            if (atData.Count > 2)
            {
                var headers = atData[1];
                atIdCol = Array.IndexOf(headers, "#");
                atNameCol = Array.IndexOf(headers, "Name");
                for (int i = 3; i < atData.Count; i++)
                {
                    if (atData[i].Length > Math.Max(atIdCol, atNameCol) && atIdCol >= 0 && atNameCol >= 0)
                    {
                        if (int.TryParse(atData[i][atIdCol], out int id))
                        {
                            attackTypes[id] = atData[i][atNameCol];
                        }
                    }
                }
            }

            // ContentFinderCondition Dictionary
            var cfConditions = new Dictionary<int, string>();
            var cfcData = await DownloadCsvLinesAsync(baseUrl + "ContentFinderCondition.csv");
            int cfcIdCol = -1, cfcNameCol = -1;
            // Get columns (some repositories have TerritoryType column)
            int cfcTerritoryTypeCol = -1;
            if (cfcData.Count > 2)
            {
                var headers = cfcData[1];
                cfcIdCol = Array.IndexOf(headers, "#");
                cfcNameCol = Array.IndexOf(headers, "Name");
                cfcTerritoryTypeCol = Array.IndexOf(headers, "TerritoryType");
                for (int i = 3; i < cfcData.Count; i++)
                {
                    if (cfcData[i].Length > Math.Max(cfcIdCol, cfcNameCol) && cfcIdCol >= 0 && cfcNameCol >= 0)
                    {
                        // Mimic Lumina logic: check if TerritoryType is valid and Name is not empty
                        bool territoryValid = false;
                        if (cfcTerritoryTypeCol >= 0 && cfcData[i].Length > cfcTerritoryTypeCol)
                        {
                            if (int.TryParse(cfcData[i][cfcTerritoryTypeCol], out int tId) && tId > 0)
                            {
                                territoryValid = true;
                            }
                        }
                        
                        string name = cfcData[i][cfcNameCol];
                        if (territoryValid && !string.IsNullOrEmpty(name))
                        {
                            if (int.TryParse(cfcData[i][cfcIdCol], out int id))
                            {
                                cfConditions[id] = name;
                            }
                        }
                    }
                }
            }

            // Action
            var actionData = await DownloadCsvLinesAsync(baseUrl + "Action.csv");
            if (actionData.Count > 2)
            {
                string text = "key,0,1,2\n#,Name,AttackType,Recast<100ms>\nint32,str,int32,int32\n";
                var headers = actionData[1];
                int idCol = Array.IndexOf(headers, "#");
                int nameCol = Array.IndexOf(headers, "Name");
                int atCol = Array.IndexOf(headers, "AttackType");
                int recastCol = Array.IndexOf(headers, "Recast<100ms>");

                for (int i = 3; i < actionData.Count; i++)
                {
                    var row = actionData[i];
                    if (row.Length <= Math.Max(Math.Max(idCol, nameCol), Math.Max(atCol, recastCol)) || idCol < 0) continue;

                    string rowId = row[idCol];
                    string name = row[nameCol];
                    string attackTypeRaw = row[atCol];
                    string recast = row[recastCol];

                    if (int.TryParse(attackTypeRaw, out int atId) && atId > 0 && atId < 9)
                    {
                        string atName = attackTypes.ContainsKey(atId) ? attackTypes[atId] : "";
                        text += rowId + ",\"" + name + "\"," + atName + "," + recast + ",\n";
                    }
                    else
                    {
                        text += rowId + ",\"" + name + "\",," + recast + ",\n";
                    }
                }
                File.WriteAllText(@"Action" + ext, text, System.Text.Encoding.UTF8);
            }

            // Status
            var statusData = await DownloadCsvLinesAsync(baseUrl + "Status.csv");
            if (statusData.Count > 2)
            {
                string text = "key,0\n#,Name\nint32,str\n";
                var headers = statusData[1];
                int idCol = Array.IndexOf(headers, "#");
                int nameCol = Array.IndexOf(headers, "Name");

                for (int i = 3; i < statusData.Count; i++)
                {
                    var row = statusData[i];
                    if (row.Length <= Math.Max(idCol, nameCol) || idCol < 0) continue;
                    text += row[idCol] + ",\"" + row[nameCol] + "\"\n";
                }
                File.WriteAllText(@"Status" + ext, text, System.Text.Encoding.UTF8);
            }

            // TerritoryType
            var territoryData = await DownloadCsvLinesAsync(baseUrl + "TerritoryType.csv");
            if (territoryData.Count > 2)
            {
                string text = "key,0,1,2\n#,TerritoryIntendedUse,Resident,ContentFinderCondition\nint32,TerritoryIntendedUse,uint16,ContentFinderCondition\n";
                var headers = territoryData[1];
                int idCol = Array.IndexOf(headers, "#");
                int useCol = Array.IndexOf(headers, "TerritoryIntendedUse");
                int residentCol = Array.IndexOf(headers, "Resident");
                int cfcCol = Array.IndexOf(headers, "ContentFinderCondition");

                for (int i = 3; i < territoryData.Count; i++)
                {
                    var row = territoryData[i];
                    if (row.Length <= Math.Max(Math.Max(idCol, useCol), Math.Max(residentCol, cfcCol)) || idCol < 0) continue;

                    string rowId = row[idCol];
                    string intendedUse = row[useCol];
                    string resident = row[residentCol];
                    string cfcRaw = row[cfcCol];

                    if (int.TryParse(cfcRaw, out int cfcId) && cfcId != 0 && cfConditions.ContainsKey(cfcId))
                    {
                        string cfcName = cfConditions[cfcId];
                        if (!string.IsNullOrEmpty(cfcName))
                        {
                            text += rowId + ",\"" + intendedUse + "\",\"" + resident + "\",\"" + cfcName + "\"\n";
                        }
                    }
                }
                File.WriteAllText(@"TerritoryType" + ext, text, System.Text.Encoding.UTF8);
            }
        }

        private async Task<List<string[]>> DownloadCsvLinesAsync(string url)
        {
            var lines = new List<string[]>();
            try
            {
                using (var stream = await httpClient.GetStreamAsync(url))
                using (var parser = new TextFieldParser(stream))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    parser.HasFieldsEnclosedInQuotes = true;
                    while (!parser.EndOfData)
                    {
                        try
                        {
                            lines.Add(parser.ReadFields());
                        }
                        catch
                        {
                            // Ignore malformed lines
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to download {url}: {ex.Message}");
            }
            return lines;
        }
    }
}
