using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using System.Diagnostics;

namespace HojoringSupportTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            try
            {
                var ext = "";
                LuminaOptions options = new LuminaOptions();
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
                }
                if (radioButton6.Checked)
                {
                    options.DefaultExcelLanguage = Language.ChineseSimplified;
                    ext = ".zh-CN.csv";
                }
                if (radioButton7.Checked)
                {
                    options.DefaultExcelLanguage = Language.ChineseTraditional;
                    ext = ".zh-CN.csv";
                }
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
                        text = "key,0,1,2\n#,Name,AttackType,Recast100ms\nint32,str,int32,int32\n";
                        foreach (var item in sheet_a)
                        {
                            if (item.AttackType.RowId > 0 && item.AttackType.RowId < 9)
                            {
                                text += item.RowId + ",\"" + item.Name.ToString() + "\"," + sheet_at.GetRow(item.AttackType.RowId).Name.ExtractText() + "," + item.Recast100ms.ToString() + "\n";
                            }
                            else
                            {
                                text += item.RowId + ",\"" + item.Name.ToString() + "\",\n";
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
                        ContentFinderCondition cfc = item.ContentFinderCondition.Value;
                        RowRef<TerritoryType> territory = cfc.TerritoryType;
                        RowRef<TerritoryIntendedUse> intendedUse = item.TerritoryIntendedUse;
                        
                        if (territory.IsValid && !cfc.Name.IsEmpty)
                        {
                            text += item.RowId + ",\"" + intendedUse.RowId + "\",\"" + item.Resident + "\",\"" + cfc.Name.ToString() + "\"\n";
                        }
                    }
                    File.WriteAllText(@"TerritoryType" + ext, text, System.Text.Encoding.UTF8);
                }

                //MessageBox.Show("finished.");
            } catch
            {

            }

            button1.Enabled = true;
        }
    }
}
