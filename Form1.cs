using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using System.Globalization;

//Version 2.0

namespace VJInfomationManager210220
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadOptions();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            SaveOptions();
            new DataServer().CreateTable();

            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "txt file|*.txt";
            open.ShowDialog();
            string[] inputlist = new string[0];
            try
            {
                inputlist = File.ReadAllLines(@open.FileName);
            }
            catch
            {
                MessageBox.Show("File lỗi!");
            }

            int ca = 0;
            int SuccessTimes = 0;
            int FailedTimes = 0;
            //bool t1Running = true;

            Thread t1 = new Thread(delegate ()
            {

                //int Duplicate = 0;
                List<VietJetInfomation1> ListVJI1 = new List<VietJetInfomation1>();

                for (int a = 0; a < inputlist.Length; a++)
                {
                    ca = a;

                    bool Success = true;

                    //Kiểm tra null, độ dài khi split |
                    string[] currentinfo = new string[0];
                    if (inputlist[a] != null || inputlist[a].Split('|').Length >= 4)
                        currentinfo = inputlist[a].Split('|');

                    //Nếu mã khách hàng null,"" thì bỏ qua
                    string customeriscode = null;
                    if (currentinfo[0] == null || currentinfo[0] == "")
                    {
                        FailedTimes++;
                        continue;
                    }
                    customeriscode = currentinfo[0];

                    //Nếu họ,tên null và split | !=2 thì bỏ qua
                    string firstname = null;
                    string lastname = "";
                    if (currentinfo[1] != null && currentinfo[1] != "" && currentinfo[1].Split(',').Length == 2)
                    {
                        string[] FirstLastName = currentinfo[1].Split(',');
                        //Nếu họ null
                        if (FirstLastName[0] == null || FirstLastName[0] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        firstname = FirstLastName[0].Replace(" ", "");


                        if (FirstLastName[1] == null || FirstLastName[1] == "")
                        {
                            FailedTimes++;
                            continue;
                        }

                        string[] HandleLastName = FirstLastName[1].Split(' ');
                        for (int b = 0; b < HandleLastName.Length; b++)
                        {
                            HandleLastName[b] = HandleLastName[b].Replace(" ", "");
                            if (HandleLastName[b] != null && HandleLastName[b] != "")
                            {
                                lastname += HandleLastName[b];
                                if (b + 1 != HandleLastName.Length)
                                    lastname += " ";
                            }
                        }
                    }
                    else
                    {
                        FailedTimes++;
                        continue;
                    }

                    //Nếu định dạng ngày,mã bay,xác nhận không đúng định dạng -> skip
                    string dateflight1 = "";
                    string flight1 = "";
                    string flightcode1 = "";
                    MatchCollection coll = Regex.Matches(currentinfo[2], "(\\d{2}/\\d{2}/\\d{4}) ([A-Z]{1,3} - [A-Z]{1,3}) ([A-Z]{1,5})");
                    try
                    {
                        if (coll.Count < 1 || coll[0].Groups.Count < 4)
                        {
                            FailedTimes++;
                            continue;
                        }
                        dateflight1 = coll[0].Groups[1].Value;
                        flight1 = coll[0].Groups[2].Value;
                        flightcode1 = coll[0].Groups[3].Value;
                    }
                    catch (Exception d)
                    {
                        /*MessageBox.Show(a.ToString() + " _ " + d.Message + " _ " + inputlist[a]);*/
                        FailedTimes++;
                        continue;
                    }

                    string dateflight2 = "";
                    string flight2 = "";
                    string flightcode2 = "";
                    string seats = "";

                    coll = Regex.Matches(currentinfo[3], @"(\d{2}/\d{2}/\d{4}) ([A-Z]{1,3} - [A-Z]{1,3}) ([A-Z]{1,5})");
                    if (coll.Count > 0 && coll[0].Groups.Count >= 4)
                    {
                        dateflight2 = coll[0].Groups[1].Value;
                        flight2 = coll[0].Groups[2].Value;
                        flightcode2 = coll[0].Groups[3].Value;
                        if (currentinfo[4] == null || currentinfo[4] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        seats = currentinfo[4];
                    }
                    else
                    {
                        if (currentinfo[3] == null || currentinfo[3] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        seats = currentinfo[3];
                    }
                    SuccessTimes++;
                    ListVJI1.Add(new VietJetInfomation1 { CustomerIsCode = customeriscode, FirstName = firstname, LastName = lastname, DateFlight1 = dateflight1, Flight1 = flight1, FlightCode1 = flightcode1, DateFlight2 = dateflight2, Flight2 = flight2, FlightCode2 = flightcode2, Seats = seats });

                }
                new DataServer().InsertNewData(ListVJI1);
            });
            t1.Start();

            Thread t2 = new Thread(delegate ()
            {
                int countseconds = 0;
                while (t1.IsAlive)
                {
                    countseconds++;
                    Thread.Sleep(100);
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button1.Text = ca.ToString() + "/" + inputlist.Length.ToString() + " " + (countseconds / 10).ToString()+"s" + Environment.NewLine + " OK:" + SuccessTimes.ToString() + " Failed:" + FailedTimes;
                        Application.DoEvents();
                    }));
                }
                Invoke((MethodInvoker)(() =>
                {
                    this.button1.Text = (countseconds / 10).ToString() + "s" + " Hoàn thành OK:" + SuccessTimes.ToString() + Environment.NewLine + " Lỗi:" + FailedTimes;
                    Application.DoEvents();
                }));
            });
            t2.Start();

        }
        List<VietJetInfomation> ListVJIFiltered;
        private void Button2_Click(object sender, EventArgs e)
        {
            SaveOptions();
            dataGridView1.DataSource = null;

            bool DisplayStt = checkBox28.Checked;
            bool DisplayCustomeriscode = checkBox29.Checked;
            bool DisplayFirstname = checkBox30.Checked;
            bool DisplayLastname = checkBox31.Checked;
            bool DisplayDateflight1t1 = checkBox32.Checked;
            bool DisplayFlight1 = checkBox33.Checked;
            bool DisplayVerify1 = checkBox34.Checked;
            bool DisplayDateflight2t1 = checkBox35.Checked;
            bool DisplayFlight2 = checkBox36.Checked;
            bool DisplayVerify2 = checkBox37.Checked;
            bool DisplaySeats = checkBox38.Checked;
            bool DisplayDateflight1t2 = checkBox39.Checked;
            bool DisplayFlightcode1 = checkBox40.Checked;
            bool DisplayDateflight2t2 = checkBox41.Checked;
            bool DisplayFlightcode2 = checkBox42.Checked;
            bool DisplayEmail = checkBox43.Checked;
            bool DisplayEmailstandardizedsuccess = checkBox44.Checked;
            bool DisplayEmailstandardized = checkBox45.Checked;
            bool DisplayPhone = checkBox46.Checked;
            bool DisplayPhonestandardizedsuccess = checkBox47.Checked;
            bool DisplayPhonestandardized = checkBox48.Checked;
            bool DisplayPhonenetwork = checkBox49.Checked;
            bool DisplayConfirm = checkBox50.Checked;
            bool DisplayPayment = checkBox51.Checked;

            var DataTable = new DataTable();
            if (DisplayStt)
                DataTable.Columns.Add("STT");
            if (DisplayCustomeriscode)
                DataTable.Columns.Add("Mã khách hàng");
            if (DisplayFirstname)
                DataTable.Columns.Add("Họ");
            if (DisplayLastname)
                DataTable.Columns.Add("(Tên đệm)Tên");
            if (DisplayDateflight1t1)
                DataTable.Columns.Add("Ngày đi");
            if (DisplayFlight1)
                DataTable.Columns.Add("Mã đi");
            if (DisplayVerify1)
                DataTable.Columns.Add("Xác nhận 1");
            if (DisplayDateflight2t1)
                DataTable.Columns.Add("Ngày về");
            if (DisplayFlight2)
                DataTable.Columns.Add("Mã về");
            if (DisplayVerify2)
                DataTable.Columns.Add("Xác nhận 2");
            if (DisplaySeats)
                DataTable.Columns.Add("Số ghế");
            if (DisplayDateflight1t2)
                DataTable.Columns.Add("Ngày 1 chiều");
            if (DisplayFlightcode1)
                DataTable.Columns.Add("Mã 1 chiều");
            if (DisplayDateflight2t2)
                DataTable.Columns.Add("Ngày 2 chiều");
            if (DisplayFlightcode2)
                DataTable.Columns.Add("Mã 2 chiều");
            if (DisplayEmail)
                DataTable.Columns.Add("Email");
            if (DisplayEmailstandardizedsuccess)
                DataTable.Columns.Add("Email C.H thành công");
            if (DisplayEmailstandardized)
                DataTable.Columns.Add("Email chuẩn hóa");
            if (DisplayPhone)
                DataTable.Columns.Add("SĐT");
            if (DisplayPhonestandardizedsuccess)
                DataTable.Columns.Add("SĐT C.H thành công");
            if (DisplayPhonestandardized)
                DataTable.Columns.Add("SĐT chuẩn hóa");
            if (DisplayPhonenetwork)
                DataTable.Columns.Add("Nhà mạng");
            if (DisplayConfirm)
                DataTable.Columns.Add("Xác nhận");
            if (DisplayPayment)
                DataTable.Columns.Add("Thanh toán");

            bool FirstNameCountryVietNam = checkBox1.Checked;
            string[] ListFirstNameCountryVietNam = File.ReadAllLines(Application.StartupPath + "//firstnamevn.txt");


            bool FirstNameOtherCountry = checkBox2.Checked;
            bool TimeLineChecked = checkBox3.Checked;
            string FromTime = textBox1.Text;
            string ToTime = textBox2.Text;

            bool Conf = checkBox14.Checked;
            bool Canx = checkBox15.Checked;

            bool OneWayTrip = checkBox11.Checked;
            bool TwoWayTrip = checkBox12.Checked;

            bool CodeFrom = checkBox4.Checked;
            string CodeFrom1 = textBox3.Text;
            bool CodeTo = checkBox5.Checked;
            string CodeTo1 = textBox4.Text;

            bool NoGetFirstName = checkBox16.Checked;
            string NoFirstNameGet = textBox9.Text;

            bool NoGetLastName = checkBox17.Checked;
            string NoLastNameGet = textBox10.Text;

            bool OnlyEmailStandardizedSuccess = checkBox18.Checked;
            bool OnlyPhoneStandardizedSuccess = checkBox19.Checked;

            bool EmailBlackListChecked = checkBox20.Checked;
            bool PhoneBlackListChecked = checkBox21.Checked;
            string[] EmailBlackList = File.ReadAllLines(Application.StartupPath + "//emailblacklist.txt");
            string[] PhoneBlackList = File.ReadAllLines(Application.StartupPath + "//phoneblacklist.txt");

            bool GetHaveEmail = checkBox22.Checked;
            bool GetHavePhone = checkBox23.Checked;

            bool OnlyPhoneNetwork = checkBox24.Checked;
            string PhoneNetWork = textBox11.Text;

            bool OnlyEmailLoop = checkBox52.Checked;
            int EmailLoop = Convert.ToInt32(numericUpDown1.Value);
            bool OnlyPhoneLoop = checkBox53.Checked;
            int PhoneLoop = Convert.ToInt32(numericUpDown2.Value);

            bool OnlyNoPhone = checkBox54.Checked;
            bool OnlyNoEmail = checkBox55.Checked;

            int loop = 1;

            Thread t3 = new Thread(delegate ()
            {
                Invoke((MethodInvoker)(() =>
                {
                    this.button2.Text = "Đang tải dữ liệu...";
                    Application.DoEvents();
                }));
                List<VietJetInfomation> ListAllVJI = new List<VietJetInfomation>();
                try
                {
                    ListAllVJI = new DataServer().LoadAllData();
                }
                catch { }
                List<VietJetInfomation2> ListAllVJI2 = new List<VietJetInfomation2>();
                try
                {
                    ListAllVJI2 = new DataServer().LoadAllDataVJ2();
                }
                catch { }
                ListVJIFiltered = new List<VietJetInfomation>();
                for (int a = ListAllVJI2.Count - 1; a > -1; a--)
                {
                    int index = ListAllVJI.IndexOf(ListAllVJI.Where(p => p.CustomerIsCode == ListAllVJI2[a].CustomerIsCode).FirstOrDefault());
                    if (index < 0)
                        continue;
                    ListAllVJI[index].DateFlight1t2 = ListAllVJI2[a].DateFlight1t2;
                    ListAllVJI[index].FlightCode1 = ListAllVJI2[a].FlightCode1;
                    ListAllVJI[index].DateFlight2t2 = ListAllVJI2[a].DateFlight2t2;
                    ListAllVJI[index].FlightCode2 = ListAllVJI2[a].FlightCode2;
                    ListAllVJI[index].Email = ListAllVJI2[a].Email;
                    ListAllVJI[index].EmailStandardizedSuccess = ListAllVJI2[a].EmailStandardizedSuccess;
                    ListAllVJI[index].EmailStandardized = ListAllVJI2[a].EmailStandardized;
                    ListAllVJI[index].Phone = ListAllVJI2[a].Phone;
                    ListAllVJI[index].PhoneStandardizedSuccess = ListAllVJI2[a].PhoneStandardizedSuccess;
                    ListAllVJI[index].PhoneStandardized = ListAllVJI2[a].PhoneStandardized;
                    ListAllVJI[index].PhoneNetwork = ListAllVJI2[a].PhoneNetwork;
                    ListAllVJI[index].Confirm = ListAllVJI2[a].Confirm;
                    ListAllVJI[index].PaymentStatus = ListAllVJI2[a].PaymentStatus;
                    ListAllVJI2.Remove(ListAllVJI2[a]);
                }
                for (int a = 0; a < ListAllVJI2.Count; a++)
                    ListAllVJI.Add(new VietJetInfomation { CustomerIsCode = ListAllVJI2[a].CustomerIsCode, FirstName = ListAllVJI2[a].FirstName, LastName = ListAllVJI2[a].LastName, DateFlight1t2 = ListAllVJI2[a].DateFlight1t2, FlightCode1 = ListAllVJI2[a].FlightCode1, DateFlight2t2 = ListAllVJI2[a].DateFlight2t2, FlightCode2 = ListAllVJI2[a].FlightCode2, Email = ListAllVJI2[a].Email, EmailStandardizedSuccess = ListAllVJI2[a].EmailStandardizedSuccess, EmailStandardized = ListAllVJI2[a].EmailStandardized, Phone = ListAllVJI2[a].Phone, PhoneStandardizedSuccess = ListAllVJI2[a].PhoneStandardizedSuccess, PhoneStandardized = ListAllVJI2[a].PhoneStandardized, PhoneNetwork = ListAllVJI2[a].PhoneNetwork, Confirm = ListAllVJI2[a].Confirm, PaymentStatus = ListAllVJI2[a].PaymentStatus });

                Invoke((MethodInvoker)(() =>
                {
                    this.button2.Text = "Họ,tên...";
                    Application.DoEvents();
                }));
                for (int a = ListAllVJI.Count - 1; a > -1; a--)
                {
                    VietJetInfomation vji = ListAllVJI[a];
                    if (FirstNameCountryVietNam && !FirstNameOtherCountry)
                    {
                        bool IsFirstNameVietNam = false;
                        for (int b = 0; b < ListFirstNameCountryVietNam.Length; b++)
                        {
                            if (vji.FirstName.ToLower() == ListFirstNameCountryVietNam[b].ToLower())
                            {
                                IsFirstNameVietNam = true;
                                break;
                            }
                        }
                        if (!IsFirstNameVietNam)
                            ListAllVJI.RemoveAt(a);
                    }
                    else if (!FirstNameCountryVietNam && FirstNameOtherCountry)
                    {
                        bool IsFirstNameVietNam = false;
                        for (int b = 0; b < ListFirstNameCountryVietNam.Length; b++)
                        {
                            if (vji.FirstName.ToLower() == ListFirstNameCountryVietNam[b].ToLower())
                            {
                                IsFirstNameVietNam = true;
                                break;
                            }
                        }
                        if (IsFirstNameVietNam)
                            ListAllVJI.RemoveAt(a);
                    }
                    else if (FirstNameCountryVietNam && FirstNameOtherCountry) { }
                    else if (!FirstNameCountryVietNam && !FirstNameOtherCountry) { ListAllVJI.RemoveAt(a); }
                }

                if (TimeLineChecked)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = " Mốc thời gian...";
                        Application.DoEvents();
                    }));
                    string[] FromTimeSplit = FromTime.Split('/');
                    string[] ToTimeSplit = ToTime.Split('/');
                    for (int a = ListAllVJI.Count - 1; a>=  0; a--)
                    {
                        if (a % 1000 == 0)
                            Invoke((MethodInvoker)(() =>
                            {
                                this.button2.Text = " Mốc thời gian " + a.ToString();
                                Application.DoEvents();
                            }));
                        VietJetInfomation vji = ListAllVJI[a];
                        //try
                        //{
                        string[] CurrentTime1 = new string[0];
                        if (vji.DateFlight1t1!=null)
                            CurrentTime1 = vji.DateFlight1t1.Split('/');
                        string[] CurrentTime2 = new string[0];
                        if (vji.DateFlight2t1 != null)
                            CurrentTime2 = vji.DateFlight2t1.Split('/');
                        if (CurrentTime1.Length == 3 && Convert.ToInt32((CurrentTime1[2] + CurrentTime1[1] + CurrentTime1[0])) >= Convert.ToInt32((FromTimeSplit[2] + FromTimeSplit[1] + FromTimeSplit[0])) && Convert.ToInt32((CurrentTime1[2] + CurrentTime1[1] + CurrentTime1[0])) <= Convert.ToInt32(ToTimeSplit[2] + ToTimeSplit[1] + ToTimeSplit[0]))
                        {
                            if (CurrentTime2.Length == 3 && Convert.ToInt32((CurrentTime2[2] + CurrentTime2[1] + CurrentTime2[0])) >= Convert.ToInt32((FromTimeSplit[2] + FromTimeSplit[1] + FromTimeSplit[0])) && Convert.ToInt32((CurrentTime2[2] + CurrentTime2[1] + CurrentTime2[0])) <= Convert.ToInt32(ToTimeSplit[2] + ToTimeSplit[1] + ToTimeSplit[0]))
                            { }
                            else ListAllVJI.RemoveAt(a);
                        }
                        else ListAllVJI.RemoveAt(a);
                        //}
                        //catch { ListAllVJI.RemoveAt(a); }
                    }
                }
                //MessageBox.Show(ListAllVJI.Count.ToString());

                Invoke((MethodInvoker)(() =>
                {
                    this.button2.Text = "CONF,CANX...";
                    Application.DoEvents();
                }));
                for (int a = ListAllVJI.Count - 1; a >= 0; a--)
                {
                    VietJetInfomation vji = ListAllVJI[a];
                    if (Conf && !Canx)
                    {
                        if (vji.Verify1 != "CONF")
                            ListAllVJI.RemoveAt(a);
                    }
                    else if (!Conf && Canx)
                    {
                        if (vji.Verify1 != "CANX")
                            ListAllVJI.RemoveAt(a);
                    }
                    else if (Conf && Canx) { }
                    else if (!Conf && !Canx) { ListAllVJI.RemoveAt(a); }
                }

                Invoke((MethodInvoker)(() =>
                {
                    this.button2.Text = "1,2 chiều...";
                    Application.DoEvents();
                }));
                for (int a = ListAllVJI.Count - 1; a > -1; a--)
                {
                    VietJetInfomation vji = ListAllVJI[a];
                    if (OneWayTrip && !TwoWayTrip)
                    {
                        if (vji.DateFlight1t1 == "" || vji.DateFlight1t1 == null)
                            ListAllVJI.RemoveAt(a);
                        if (vji.DateFlight2t1 != "")
                            if (vji.DateFlight2t1 != null)
                                ListAllVJI.RemoveAt(a);
                    }
                    else if (!OneWayTrip && TwoWayTrip)
                    {
                        if (vji.DateFlight2t1 == "" || vji.DateFlight2t1 == null)
                            ListAllVJI.RemoveAt(a);
                    }
                    else if (OneWayTrip && TwoWayTrip)
                    {
                        //if (CurrentVJI.DateFlight1 == "" || CurrentVJI.DateFlight1 == null)
                        //    if (CurrentVJI.DateFlight2 == "" || CurrentVJI.DateFlight2 == null)
                        //        continue;
                    }
                    else if (!OneWayTrip && !TwoWayTrip)
                    {
                        ListAllVJI.RemoveAt(a);
                    }
                }

                if (CodeFrom)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Đi từ...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        bool match = false;
                        string[] ListCodeFrom1 = CodeFrom1.Split(',');
                        for (int c = 0; c < ListCodeFrom1.Length; c++)
                        {
                            if (vji.Flight1 != null && Regex.IsMatch(vji.Flight1, ListCodeFrom1[c] + " -"))
                            {
                                match = true;
                                break;
                            }else if(vji.Flight2 != null && Regex.IsMatch(vji.Flight2, ListCodeFrom1[c] + " -"))
                            {
                                match = true;
                                break;
                            }
                        }
                        if (!match)
                            ListAllVJI.RemoveAt(a);
                    }
                }

                if (CodeTo)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Đi tới...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        bool match = false;
                        string[] ListCodeTo1 = CodeTo1.Split(',');
                        for (int c = 0; c < ListCodeTo1.Length; c++)
                        {
                            if (vji.Flight2 != null && Regex.IsMatch(vji.Flight1, "- " + ListCodeTo1[c]))
                            {
                                match = true;
                                break;
                            }
                            else if (vji.Flight2 != null && Regex.IsMatch(vji.Flight2, "- " + ListCodeTo1[c]))
                            {
                                match = true;
                                break;
                            }
                        }
                        if (!match)
                            ListAllVJI.RemoveAt(a);
                    }
                }

                if (NoGetFirstName)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Không lấy họ...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        if (vji.FirstName == NoFirstNameGet)
                            ListAllVJI.RemoveAt(a);
                    }
                }

                if (NoGetLastName)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Không lấy tên...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        if (vji.LastName != NoLastNameGet)
                            ListAllVJI.RemoveAt(a);
                    }
                }

                if (OnlyEmailStandardizedSuccess)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Chỉ lấy email C.H thành công...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        if (!Convert.ToBoolean(vji.EmailStandardizedSuccess))
                            ListAllVJI.RemoveAt(a);
                    }
                }

                if (OnlyPhoneStandardizedSuccess)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Chỉ lấy SĐT C.H thành công...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        if (!Convert.ToBoolean(vji.PhoneStandardizedSuccess))
                            ListAllVJI.RemoveAt(a);
                    }
                }

                if (EmailBlackListChecked)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Loại bỏ Email blacklist...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        if (Convert.ToBoolean(vji.EmailStandardizedSuccess))
                        {
                            bool IsBlackList = false;
                            for (int d = 0; d < EmailBlackList.Length; d++)
                            {
                                if (vji.EmailStandardized == EmailBlackList[d])
                                {
                                    IsBlackList = true;
                                    break;
                                }
                            }
                            if (IsBlackList)
                                ListAllVJI.RemoveAt(a);
                        }
                    }
                }

                if (PhoneBlackListChecked)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Loại bỏ SĐT blacklist...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        if (Convert.ToBoolean(vji.PhoneStandardizedSuccess))
                        {
                            bool IsBlackList = false;
                            for (int d = 0; d < PhoneBlackList.Length; d++)
                            {
                                if (vji.PhoneStandardized == PhoneBlackList[d])
                                {
                                    IsBlackList = true;
                                    break;
                                }
                            }
                            if (IsBlackList)
                                ListAllVJI.RemoveAt(a);
                        }
                    }
                }

                if (GetHaveEmail)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Chỉ lấy có Email...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        if (vji.Email == null || vji.Email == "")
                            ListAllVJI.RemoveAt(a);
                    }
                }

                if (GetHavePhone)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Chỉ lấy có SĐT...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        if (vji.Phone == null || vji.Phone == "")
                            ListAllVJI.RemoveAt(a);
                    }
                }
                if (GetHaveEmail)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Chỉ lấy có Email...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        if (vji.Email == null || vji.Email == "")
                            ListAllVJI.RemoveAt(a);
                    }
                }

                if (OnlyNoEmail)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Không lấy có Email...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        if (vji.Email == null || vji.Email == "")
                            ListAllVJI.RemoveAt(a);
                    }
                }


                if (OnlyNoPhone)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Không lấy có SĐT...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                    {
                        VietJetInfomation vji = ListAllVJI[a];
                        if (vji.Phone == null || vji.Phone == "")
                            ListAllVJI.RemoveAt(a);
                    }
                }

                if (OnlyEmailLoop)
                {

                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Lấy email đại lý...";
                        Application.DoEvents();
                    }));

                    for (int a = ListAllVJI.Count - 1; a > -1; a--)
                        if (!Convert.ToBoolean(ListAllVJI[a].EmailStandardizedSuccess))
                            ListAllVJI.RemoveAt(a);
                    ListAllVJI.Sort((x, y) => x.EmailStandardized.CompareTo(y.EmailStandardized));
                    for (int a = ListAllVJI.Count - 1; a > -1;)
                    {
                        if (a % 1000 == 0)
                            Invoke((MethodInvoker)(() =>
                            {
                                this.button2.Text = "Lấy email đại lý " + a.ToString();
                                Application.DoEvents();
                            }));
                        VietJetInfomation vji = ListAllVJI[a];
                        int c = 0;
                        //MessageBox.Show(vji.FirstName);
                        for (int b = a; b >= 0;)
                        {
                            if (ListAllVJI[b].EmailStandardized == vji.EmailStandardized)
                                c++;
                            else break;
                            b--;
                        }
                        //MessageBox.Show("Lap " + c.ToString());
                        if (c < EmailLoop)
                        {
                            for (int b = a; b > (a - c);)
                            {
                                //MessageBox.Show(a.ToString() + "Xoa dong" + b.ToString());
                                ListAllVJI.RemoveAt(b);
                                b--;
                            }
                        }
                        a = a - c;
                    }
                }

                //ListAllVJI.Sort((x, y) => x.FirstName.CompareTo(y.FirstName));
                if (OnlyPhoneLoop)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button2.Text = "Lấy SĐT đại lý...";
                        Application.DoEvents();
                    }));
                    for (int a = ListAllVJI.Count - 1; a >= 0;a--)
                        if (!Convert.ToBoolean(ListAllVJI[a].PhoneStandardizedSuccess))
                            ListAllVJI.RemoveAt(a);
                    ListAllVJI.Sort((x, y) => x.PhoneStandardized.CompareTo(y.PhoneStandardized));
                    //MessageBox.Show(ListAllVJI.Count.ToString());
                    for (int a = ListAllVJI.Count - 1; a >= 0;)
                    {
                        VietJetInfomation vji = ListAllVJI[a];

                        if (a % 1000 == 0)
                            Invoke((MethodInvoker)(() =>
                            {
                                this.button2.Text = "Lấy SĐT đại lý " + a.ToString();
                                Application.DoEvents();
                            }));

                        int c = 0;
                        for (int b = a; b >= 0; )
                        {
                            if (ListAllVJI[b].PhoneStandardized == vji.PhoneStandardized)
                                c++;
                            else break;
                            b--;
                        }
                        if (c < PhoneLoop)
                        {
                            for (int b = a; b > (a - c); )
                            {
                                ListAllVJI.RemoveAt(b);
                                b--;
                            }
                        }
                        a = a - c;
                    }
                }

                for (int a = 0; a < ListAllVJI.Count; a++)
                {
                    VietJetInfomation vji = ListAllVJI[a];
                    DataRow datar = DataTable.NewRow();
                    if (DisplayStt)
                        datar["STT"] = loop;
                    loop++;
                    if (DisplayCustomeriscode)
                        datar["Mã khách hàng"] = vji.CustomerIsCode.ToString();
                    if (DisplayFirstname)
                        datar["Họ"] = vji.FirstName;
                    if (DisplayLastname)
                        datar["(Tên đệm)Tên"] = vji.LastName;
                    if (DisplayDateflight1t1)
                        datar["Ngày đi"] = vji.DateFlight1t1;
                    if (DisplayFlight1)
                        datar["Mã đi"] = vji.Flight1;
                    if (DisplayVerify1)
                        datar["Xác nhận 1"] = vji.Verify1;
                    if (DisplayDateflight2t1)
                        datar["Ngày về"] = vji.DateFlight2t1;
                    if (DisplayFlight2)
                        datar["Mã về"] = vji.Flight2;
                    if (DisplayVerify2)
                        datar["Xác nhận 2"] = vji.Verify2;
                    if (DisplaySeats)
                        datar["Số ghế"] = vji.Seats;
                    if (DisplayDateflight1t2)
                        datar["Ngày 1 chiều"] = vji.DateFlight1t2;
                    if (DisplayFlightcode1)
                        datar["Mã 1 chiều"] = vji.FlightCode1;
                    if (DisplayDateflight2t2)
                        datar["Ngày 2 chiều"] = vji.DateFlight2t2;
                    if (DisplayFlightcode2)
                        datar["Mã 2 chiều"] = vji.FlightCode2;
                    if (DisplayEmail)
                        datar["Email"] = vji.Email;
                    if (DisplayEmailstandardizedsuccess)
                        datar["Email C.H thành công"] = vji.EmailStandardizedSuccess;
                    if (DisplayEmailstandardized)
                        datar["Email chuẩn hóa"] = vji.EmailStandardized;
                    if (DisplayPhone)
                        datar["SĐT"] = vji.Phone;
                    if (DisplayPhonestandardizedsuccess)
                        datar["SĐT C.H thành công"] = vji.PhoneStandardizedSuccess;
                    if (DisplayPhonestandardized)
                        datar["SĐT chuẩn hóa"] = vji.PhoneStandardized;
                    if (DisplayPhonenetwork)
                        datar["Nhà mạng"] = vji.PhoneNetwork;
                    if (DisplayConfirm)
                        datar["Xác nhận"] = vji.Confirm;
                    if (DisplayPayment)
                        datar["Thanh toán"] = vji.PaymentStatus;
                    DataTable.Rows.Add(datar);
                    ListVJIFiltered.Add(new VietJetInfomation { CustomerIsCode = vji.CustomerIsCode, FirstName = vji.FirstName, LastName = vji.LastName, DateFlight1t1 = vji.DateFlight1t1, Flight1 = vji.Flight1, Verify1 = vji.Verify1, DateFlight2t1 = vji.DateFlight2t1, Flight2 = vji.Flight2, Verify2 = vji.Verify2, DateFlight1t2 = vji.DateFlight1t2, FlightCode1 = vji.FlightCode1, DateFlight2t2 = vji.DateFlight2t2, Email = vji.Email, EmailStandardizedSuccess = vji.EmailStandardizedSuccess, EmailStandardized = vji.EmailStandardized, Phone = vji.Phone, PhoneStandardizedSuccess = vji.PhoneStandardizedSuccess, PhoneStandardized = vji.PhoneStandardized, PhoneNetwork = vji.PhoneNetwork, Confirm = vji.Confirm, PaymentStatus = vji.PaymentStatus });
                }
                Invoke((MethodInvoker)(() =>
                    {
                        this.dataGridView1.DataSource = DataTable;
                        dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        this.button2.Text = "Hoàn thành " + DataTable.Rows.Count.ToString();
                        Application.DoEvents();
                    }));
            });
            t3.Start();
        }

    private void Button3_Click(object sender, EventArgs e)
        {
            SaveOptions();
            string[] Export = new string[0];
            bool FlightCode = checkBox6.Checked;
            bool FirstName = checkBox7.Checked;
            bool LastName = checkBox8.Checked;
            bool Flight = checkBox9.Checked;
            bool Seats = checkBox10.Checked;
            bool Date = checkBox13.Checked;
            bool Email = checkBox25.Checked;
            bool Phone = checkBox26.Checked;
            bool PhoneNetwork = checkBox27.Checked;
            Invoke((MethodInvoker)(() =>
            {
                this.button3.Text = "Đang tiến hành...";
                Application.DoEvents();
            }));
            Thread t2 = new Thread(delegate ()
            {
                for (int a = 0; a < ListVJIFiltered.Count; a++)
                {
                    Array.Resize(ref Export, Export.Length + 1);
                    if (FlightCode)
                        Export[Export.Length - 1] += ListVJIFiltered[a].CustomerIsCode + "|";
                    if (FirstName)
                        Export[Export.Length - 1] += ListVJIFiltered[a].FirstName + "|";
                    if (LastName)
                        Export[Export.Length - 1] += ListVJIFiltered[a].LastName + "|";
                    if (Date)
                        Export[Export.Length - 1] += ListVJIFiltered[a].DateFlight1t1 + "|";
                    if (Flight)
                        Export[Export.Length - 1] += ListVJIFiltered[a].Flight1 + "|";
                    if (Date && ListVJIFiltered[a].DateFlight2t1 != null && ListVJIFiltered[a].DateFlight2t1 != "")
                        Export[Export.Length - 1] += ListVJIFiltered[a].DateFlight2t1 + "|";
                    if (Flight && ListVJIFiltered[a].Flight2 != null && ListVJIFiltered[a].DateFlight2t1 != "")
                        Export[Export.Length - 1] += ListVJIFiltered[a].Flight2 + "|";
                    if (Seats)
                        Export[Export.Length - 1] += ListVJIFiltered[a].Seats + "|";
                    if (Email)
                        Export[Export.Length - 1] += ListVJIFiltered[a].Email + "|";
                    if (Phone)
                        Export[Export.Length - 1] += ListVJIFiltered[a].Phone + "|";
                    if (PhoneNetwork)
                        Export[Export.Length - 1] += ListVJIFiltered[a].PhoneNetwork + "|";
                    Export[Export.Length - 1].Remove(Export[Export.Length - 1].Length - 1);
                }
                File.WriteAllLines(Application.StartupPath + "//Export.txt", Export);
                Export = new string[0];
                Invoke((MethodInvoker)(() =>
                {
                    this.button3.Text = "Thành công " + ListVJIFiltered.Count.ToString();
                    Application.DoEvents();
                }));
            });
            t2.Start();
        }
        public void LoadOptions()
        {
            try
            {
                string[] Options = File.ReadAllLines(Application.StartupPath + "//option");
                //InputData.MaxThread = Convert.ToInt32(Options[0]);
                checkBox1.Checked = Convert.ToBoolean(Options[1]);
                checkBox2.Checked = Convert.ToBoolean(Options[2]);
                checkBox3.Checked = Convert.ToBoolean(Options[3]);
                checkBox4.Checked = Convert.ToBoolean(Options[4]);
                checkBox5.Checked = Convert.ToBoolean(Options[5]);
                checkBox6.Checked = Convert.ToBoolean(Options[6]);
                checkBox7.Checked = Convert.ToBoolean(Options[7]);
                checkBox8.Checked = Convert.ToBoolean(Options[8]);
                checkBox9.Checked = Convert.ToBoolean(Options[9]);
                checkBox10.Checked = Convert.ToBoolean(Options[10]);
                checkBox11.Checked = Convert.ToBoolean(Options[11]);
                checkBox12.Checked = Convert.ToBoolean(Options[12]);
                checkBox13.Checked = Convert.ToBoolean(Options[13]);
                checkBox14.Checked = Convert.ToBoolean(Options[14]);
                checkBox15.Checked = Convert.ToBoolean(Options[15]);
                textBox1.Text = Options[16];
                textBox2.Text = Options[17];
                textBox3.Text = Options[18];
                textBox4.Text = Options[19];
                textBox5.Text = Options[20];
                textBox6.Text = Options[21];
                textBox7.Text = Options[22];
                textBox8.Text = Options[23];
                textBox9.Text = Options[24];
                textBox10.Text = Options[25];
                checkBox16.Checked = Convert.ToBoolean(Options[26]);
                checkBox17.Checked = Convert.ToBoolean(Options[27]);
                checkBox18.Checked = Convert.ToBoolean(Options[28]);
                checkBox19.Checked = Convert.ToBoolean(Options[29]);
                checkBox20.Checked = Convert.ToBoolean(Options[30]);
                checkBox21.Checked = Convert.ToBoolean(Options[31]);
                checkBox22.Checked = Convert.ToBoolean(Options[32]);
                checkBox23.Checked = Convert.ToBoolean(Options[33]);
                checkBox24.Checked = Convert.ToBoolean(Options[34]);
                checkBox25.Checked = Convert.ToBoolean(Options[35]);
                checkBox26.Checked = Convert.ToBoolean(Options[36]);
                checkBox27.Checked = Convert.ToBoolean(Options[37]);
                checkBox28.Checked = Convert.ToBoolean(Options[38]);
                checkBox29.Checked = Convert.ToBoolean(Options[39]);
                checkBox30.Checked = Convert.ToBoolean(Options[40]);
                checkBox31.Checked = Convert.ToBoolean(Options[41]);
                checkBox32.Checked = Convert.ToBoolean(Options[42]);
                checkBox33.Checked = Convert.ToBoolean(Options[43]);
                checkBox34.Checked = Convert.ToBoolean(Options[44]);
                checkBox35.Checked = Convert.ToBoolean(Options[45]);
                checkBox36.Checked = Convert.ToBoolean(Options[46]);
                checkBox37.Checked = Convert.ToBoolean(Options[47]);
                checkBox38.Checked = Convert.ToBoolean(Options[48]);
                checkBox39.Checked = Convert.ToBoolean(Options[49]);
                checkBox40.Checked = Convert.ToBoolean(Options[50]);
                checkBox41.Checked = Convert.ToBoolean(Options[51]);
                checkBox42.Checked = Convert.ToBoolean(Options[52]);
                checkBox43.Checked = Convert.ToBoolean(Options[53]);
                checkBox44.Checked = Convert.ToBoolean(Options[54]);
                checkBox45.Checked = Convert.ToBoolean(Options[55]);
                checkBox46.Checked = Convert.ToBoolean(Options[56]);
                checkBox47.Checked = Convert.ToBoolean(Options[57]);
                checkBox48.Checked = Convert.ToBoolean(Options[58]);
                checkBox49.Checked = Convert.ToBoolean(Options[59]);
                checkBox50.Checked = Convert.ToBoolean(Options[60]);
                checkBox51.Checked = Convert.ToBoolean(Options[61]);
                textBox11.Text = Options[62];
            }
            catch { }
        }
        public void SaveOptions()
        {
            InputData.Server = this.textBox5.Text;
            InputData.Database = this.textBox6.Text;
            InputData.UID = this.textBox7.Text;
            InputData.Password = this.textBox8.Text;
            string[] Options = new string[63];
            //Options[0] = Convert.ToString(InputData.MaxThread);
            Options[1] = Convert.ToString(checkBox1.Checked);
            Options[2] = Convert.ToString(checkBox2.Checked);
            Options[3] = Convert.ToString(checkBox3.Checked);
            Options[4] = Convert.ToString(checkBox4.Checked);
            Options[5] = Convert.ToString(checkBox5.Checked);
            Options[6] = Convert.ToString(checkBox6.Checked);
            Options[7] = Convert.ToString(checkBox7.Checked);
            Options[8] = Convert.ToString(checkBox8.Checked);
            Options[9] = Convert.ToString(checkBox9.Checked);
            Options[10] = Convert.ToString(checkBox10.Checked);
            Options[11] = Convert.ToString(checkBox11.Checked);
            Options[12] = Convert.ToString(checkBox12.Checked);
            Options[13] = Convert.ToString(checkBox13.Checked);
            Options[14] = Convert.ToString(checkBox14.Checked);
            Options[15] = Convert.ToString(checkBox15.Checked);
            Options[16] = textBox1.Text;
            Options[17] = textBox2.Text;
            Options[18] = textBox3.Text;
            Options[19] = textBox4.Text;
            Options[20] = textBox5.Text;
            Options[21] = textBox6.Text;
            Options[22] = textBox7.Text;
            Options[23] = textBox8.Text;
            Options[24] = textBox9.Text;
            Options[25] = textBox10.Text;
            Options[26] = Convert.ToString(checkBox16.Checked);
            Options[27] = Convert.ToString(checkBox17.Checked);
            Options[28] = Convert.ToString(checkBox18.Checked);
            Options[29] = Convert.ToString(checkBox19.Checked);
            Options[30] = Convert.ToString(checkBox20.Checked);
            Options[31] = Convert.ToString(checkBox21.Checked);
            Options[32] = Convert.ToString(checkBox22.Checked);
            Options[33] = Convert.ToString(checkBox23.Checked);
            Options[34] = Convert.ToString(checkBox24.Checked);
            Options[35] = Convert.ToString(checkBox25.Checked);
            Options[36] = Convert.ToString(checkBox26.Checked);
            Options[37] = Convert.ToString(checkBox27.Checked);
            Options[38] = Convert.ToString(checkBox28.Checked);
            Options[39] = Convert.ToString(checkBox29.Checked);
            Options[40] = Convert.ToString(checkBox30.Checked);
            Options[41] = Convert.ToString(checkBox31.Checked);
            Options[42] = Convert.ToString(checkBox32.Checked);
            Options[43] = Convert.ToString(checkBox33.Checked);
            Options[44] = Convert.ToString(checkBox34.Checked);
            Options[45] = Convert.ToString(checkBox35.Checked);
            Options[46] = Convert.ToString(checkBox36.Checked);
            Options[47] = Convert.ToString(checkBox37.Checked);
            Options[48] = Convert.ToString(checkBox38.Checked);
            Options[49] = Convert.ToString(checkBox39.Checked);
            Options[50] = Convert.ToString(checkBox40.Checked);
            Options[51] = Convert.ToString(checkBox41.Checked);
            Options[52] = Convert.ToString(checkBox42.Checked);
            Options[53] = Convert.ToString(checkBox43.Checked);
            Options[54] = Convert.ToString(checkBox44.Checked);
            Options[55] = Convert.ToString(checkBox45.Checked);
            Options[56] = Convert.ToString(checkBox46.Checked);
            Options[57] = Convert.ToString(checkBox47.Checked);
            Options[58] = Convert.ToString(checkBox48.Checked);
            Options[59] = Convert.ToString(checkBox49.Checked);
            Options[60] = Convert.ToString(checkBox50.Checked);
            Options[61] = Convert.ToString(checkBox51.Checked);
            Options[62] = textBox11.Text;
            File.WriteAllLines(Application.StartupPath + "//option", Options);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            SaveOptions();
            if (new DataServer().CheckConnection())
            {
                MessageBox.Show("Kết nối thành công !");
            }
            else MessageBox.Show("Kết nối thất bại !");
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            SaveOptions();

            new DataServer().CreateTable2();

            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "txt file|*.txt";
            open.ShowDialog();
            string[] inputlist = new string[0];
            try
            {
                inputlist = File.ReadAllLines(@open.FileName);
            }
            catch
            {
                MessageBox.Show("File lỗi!");
            }

            //bool Success = false;
            int SuccessTimes = 0;
            int FailedTimes = 0;

            int ca = 0;

            Thread t1 = new Thread(delegate ()
            {
                List<VietJetInfomation2> ListVJI2 = new List<VietJetInfomation2>();

                for (int a = 0; a < inputlist.Length; a++)
                {
                    ca = a;

                    if (inputlist[a] == null || inputlist[a] == "")
                    {
                        FailedTimes++;
                        continue;
                    }

                    string[] currentinfo = inputlist[a].Split('|');

                    //Kiểm tra chiều dài và mã khách hàng
                    if (currentinfo.Length < 9 || currentinfo[0] == null || currentinfo[0] == "")
                    {
                        FailedTimes++;
                        continue;
                    }
                    string customeriscode = currentinfo[0];

                    //Kiểm tra họ
                    if (currentinfo[1] == null || currentinfo[1] == "")
                    {
                        FailedTimes++;
                        continue;
                    }
                    string firstname = currentinfo[1];

                    //Kiểm tra tên
                    if (currentinfo[2] == null || currentinfo[2] == "")
                    {
                        FailedTimes++;
                        continue;
                    }
                    string lastname = currentinfo[2];

                    //Kiểm tra ngày bay 1
                    string dateflight1 = null;
                    MatchCollection coll = Regex.Matches(currentinfo[3], "(\\d{2}/\\d{2}/\\d{4})");
                    if (coll.Count < 1 || coll[0].Groups.Count < 2)
                    {
                        FailedTimes++;
                        continue;
                    }
                    dateflight1 = coll[0].Groups[1].Value;

                    //Kiểm tra mã chuyến bay 1
                    if (currentinfo[4] == null || currentinfo[4] == "")
                    {
                        FailedTimes++;
                        continue;
                    }
                    string flightcode1 = currentinfo[4];

                    string dateflight2 = "";
                    string flightcode2 = "";

                    string email = "";
                    string phone = "";
                    string confirm = "";
                    string paymentstatus = "";

                    //Kiểm tra 11 trường . Kiểm tra ngày bay 2 chiều
                    if (currentinfo.Length >= 11)
                    {
                        //ngày bay 2
                        coll = Regex.Matches(currentinfo[5], "(\\d{2}/\\d{2}/\\d{4})");
                        if (coll.Count < 1 || coll[0].Groups.Count < 2)
                        {
                            FailedTimes++;
                            continue;
                        }
                        dateflight2 = coll[0].Groups[1].Value;

                        //mã chuyến bay 2
                        if (currentinfo[6] == null || currentinfo[6] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        flightcode2 = currentinfo[6];


                        //Email
                        if (currentinfo[7] == null || currentinfo[7] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        email = currentinfo[7];
                        //SĐT
                        if (currentinfo[8] == null || currentinfo[8] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        phone = currentinfo[8];
                        //Xác nhận
                        if (currentinfo[9] == null || currentinfo[9] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        confirm = currentinfo[9];
                        //Thanh toán
                        if (currentinfo[10] == null || currentinfo[10] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        paymentstatus = currentinfo[10];
                    }
                    else
                    {
                        //9 trường -> 1 chiều
                        //Email
                        if (currentinfo[5] == null || currentinfo[5] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        email = currentinfo[5];
                        //SĐT
                        if (currentinfo[6] == null || currentinfo[6] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        phone = currentinfo[6];
                        //Xác nhận
                        if (currentinfo[7] == null || currentinfo[7] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        confirm = currentinfo[7];
                        //Thanh toán
                        if (currentinfo[8] == null || currentinfo[8] == "")
                        {
                            FailedTimes++;
                            continue;
                        }
                        paymentstatus = currentinfo[8];
                    }

                    //Chuẩn hóa Email
                    bool emailstandardizedsuccess = false;
                    string emailstandardized = "";

                    //Chuẩn hóa SĐT
                    bool phonestandardizedsuccess = false;
                    string phonestandardized = "";
                    string phonenetwork = "";

                    //Chuẩn hóa số Email
                    emailstandardized = new Class1().EmailStandardizedHandle(email);
                    if (emailstandardized != "")
                        emailstandardizedsuccess = true;

                    //Chuẩn hóa số điện thoại
                    phonestandardized = new Class1().PhoneStandardizedHandle(phone);
                    if (phonestandardized != "")
                        phonestandardizedsuccess = true;

                    //Nhận diện nhà mạng
                    if (phonestandardizedsuccess)
                        phonenetwork = new Class1().HandlePhoneNetwork(phonestandardized);

                    ListVJI2.Add(new VietJetInfomation2 { CustomerIsCode = customeriscode, FirstName = firstname, LastName = lastname, DateFlight1t2 = dateflight1, FlightCode1 = flightcode1, DateFlight2t2 = dateflight2, FlightCode2 = flightcode2, Email = email, EmailStandardizedSuccess = Convert.ToString(emailstandardizedsuccess), EmailStandardized = emailstandardized, Phone = phone, PhoneStandardizedSuccess = Convert.ToString(phonestandardizedsuccess), PhoneStandardized = phonestandardized, PhoneNetwork = phonenetwork, Confirm = confirm, PaymentStatus = paymentstatus });
                    SuccessTimes++;
                }
                //int lastinsertedid = 
                new DataServer().InsertNewData2(ListVJI2);
            });
            t1.Start();
            Thread t2 = new Thread(delegate ()
            {
                int countseconds = 0;
                while (t1.IsAlive)
                {
                    countseconds++;
                    //MessageBox.Show(t1.IsAlive.ToString());
                    Thread.Sleep(100);
                    Invoke((MethodInvoker)(() =>
                    {
                        this.button5.Text = ca.ToString() + "/" + inputlist.Length.ToString() + " " + (countseconds / 10).ToString() + "s" + Environment.NewLine + " OK:" + SuccessTimes.ToString() + " Failed:" + FailedTimes;
                        Application.DoEvents();
                    }));
                }
                Thread.Sleep(100);
                Invoke((MethodInvoker)(() =>
                {
                    this.button5.Text = (countseconds / 10).ToString() + "s"+ " Hoàn thành OK:" + SuccessTimes.ToString() + Environment.NewLine + " Lỗi:" + FailedTimes;
                    Application.DoEvents();
                }));
            });
            t2.Start();

        }


        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            this.dataGridView1.Width = this.Width - this.tabControl1.Width - 15;
            //dataGridView1.BindingContext = new BindingContext();
        }

        private void CheckBox41_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.dataGridView1.Width = this.Width - this.tabControl1.Width - 15;
        }
    }
    class InputData
    {
        public static string Server;
        public static string Database;
        public static string UID;
        public static string Password;
        //public static int MaxThread;
    }
    class DataServer
    {
        public bool CheckConnection()
        {
            bool result = false;
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            try
            {
                conn.Open();
                result = true;
                conn.Close();
            }
            catch { }
            return result;
        }
        public void CreateTable()
        {
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS VietJetInfomation(id INT(10) NOT NULL UNIQUE AUTO_INCREMENT,flightcode VARCHAR(100) NOT NULL UNIQUE,firstname VARCHAR(100),lastname VARCHAR(100),dateflight1 VARCHAR(100),flight1 VARCHAR(100),flightcode1 VARCHAR(100),dateflight2 VARCHAR(100),flight2 VARCHAR(100),flightcode2 VARCHAR(100),seats VARCHAR(100)) ENGINE = InnoDB";
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        public void CreateTable2()
        {
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS VietJetInfomation2(id INT(10) NOT NULL UNIQUE AUTO_INCREMENT,customeriscode VARCHAR(100) NOT NULL UNIQUE,firstname VARCHAR(100),lastname VARCHAR(100),dateflight1 VARCHAR(100),flightcode1 VARCHAR(100),dateflight2 VARCHAR(100),flightcode2 VARCHAR(100),email VARCHAR(100),emailstandardizedsuccess VARCHAR(100),emailstandardized VARCHAR(100),phone VARCHAR(100),phonestandardizedsuccess VARCHAR(100),phonestandardized VARCHAR(100),phonenetwork VARCHAR(100),confirm VARCHAR(100),paymentstatus VARCHAR(100)) ENGINE = InnoDB";
            cmd.Connection = conn;
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        //public int InsertNewData(string flightcode, string firstname, string lastname, string dateflight1, string flight1, string flightcode1, string dateflight2, string flight2, string flightcode2, string seats)
        //{
        //    MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //    conn.Open();
        //    MySqlCommand cmd = new MySqlCommand();
        //    cmd.CommandText = "INSERT INTO VietJetInfomation(flightcode,firstname,lastname,dateflight1,flight1,flightcode1,dateflight2,flight2,flightcode2,seats) VALUES ('" + flightcode + "','" + firstname + "','" + lastname + "','" + dateflight1 + "','" + flight1 + "','" + flightcode1 + "','" + dateflight2 + "','" + flight2 + "','" + flightcode2 + "','" + seats + "')";
        //    cmd.Connection = conn;
        //    cmd.ExecuteNonQuery();
        //    int LastInsertedId = Convert.ToInt32(cmd.LastInsertedId);
        //    conn.Close();
        //    return LastInsertedId;
        //}
        //public void InsertNewData(string flightcode, string firstname, string lastname, string dateflight1, string flight1, string flightcode1, string dateflight2, string flight2, string flightcode2, string seats)
        //{
        //    MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //    conn.Open();
        //    MySqlCommand cmd = new MySqlCommand();
        //    cmd.CommandText = "INSERT INTO VietJetInfomation(flightcode,firstname,lastname,dateflight1,flight1,flightcode1,dateflight2,flight2,flightcode2,seats) VALUES ('" + flightcode + "','" + firstname + "','" + lastname + "','" + dateflight1 + "','" + flight1 + "','" + flightcode1 + "','" + dateflight2 + "','" + flight2 + "','" + flightcode2 + "','" + seats + "')";
        //    cmd.Connection = conn;
        //    cmd.ExecuteNonQuery();
        //    //int LastInsertedId = Convert.ToInt32(cmd.LastInsertedId);
        //    conn.Close();
        //    //return LastInsertedId;
        //}
        public void InsertNewData(List<VietJetInfomation1> ListVJI1)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");

                //MySqlCommand cmd = new MySqlCommand();
                //cmd.CommandText = "INSERT INTO VietJetInfomation(flightcode,firstname,lastname,dateflight1,flight1,flightcode1,dateflight2,flight2,flightcode2,seats) VALUES ";

                StringBuilder Command = new StringBuilder("INSERT IGNORE INTO VietJetInfomation(flightcode,firstname,lastname,dateflight1,flight1,flightcode1,dateflight2,flight2,flightcode2,seats) VALUES ");
                List<string> Rows = new List<string>();
                foreach (VietJetInfomation1 vji1 in ListVJI1)
                {
                    Rows.Add("('" + vji1.CustomerIsCode + "','" + vji1.FirstName + "','" + vji1.LastName + "','" + vji1.DateFlight1 + "','" + vji1.Flight1 + "','" + vji1.FlightCode1 + "','" + vji1.DateFlight2 + "','" + vji1.Flight2 + "','" + vji1.FlightCode2 + "','" + vji1.Seats + "')");
                }
                Command.Append(string.Join(",", Rows));
                Command.Append(";");

                conn.Open();
                using (MySqlCommand myCmd = new MySqlCommand(Command.ToString(), conn))
                {
                    myCmd.Connection = conn;
                    myCmd.CommandTimeout = 300;
                    myCmd.CommandType = CommandType.Text;
                    myCmd.ExecuteNonQuery();
                }
                conn.Close();
            }
            catch(Exception d) { MessageBox.Show(d.Message); }
        }
        public void InsertNewData2(List<VietJetInfomation2> ListVJI2)
        {
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            //MySqlCommand cmd = new MySqlCommand();
            //cmd.CommandText = "INSERT INTO VietJetInfomation2(customeriscode,firstname,lastname,dateflight1,flightcode1,dateflight2,flightcode2,email,emailstandardizedsuccess,emailstandardized,phone,phonestandardizedsuccess,phonestandardized,phonenetwork,confirm,paymentstatus) VALUES ('" + customeriscode + "','" + firstname + "','" + lastname + "','" + dateflight1 + "','" + flightcode1 + "','" + dateflight2 + "','" + flightcode2 + "','" + email + "','" + Convert.ToString(emailstandardizedsuccess) + "','"+emailstandardized+"','" + phone + "','"+Convert.ToString(phonestandardizedsuccess)+"','" + phonestandardized + "','" + phonenetwork + "','" + confirm + "','" + paymentstatus + "')";

            StringBuilder Command = new StringBuilder("INSERT IGNORE INTO VietJetInfomation2(customeriscode,firstname,lastname,dateflight1,flightcode1,dateflight2,flightcode2,email,emailstandardizedsuccess,emailstandardized,phone,phonestandardizedsuccess,phonestandardized,phonenetwork,confirm,paymentstatus) VALUES ");
            List<string> Rows = new List<string>();
            foreach (VietJetInfomation2 vji2 in ListVJI2)
            {
                Rows.Add("('" + vji2.CustomerIsCode + "','" + vji2.FirstName + "','" + vji2.LastName + "','" + vji2.DateFlight1t2 + "','" + vji2.FlightCode1 + "','" + vji2.DateFlight2t2 + "','" + vji2.FlightCode2 + "','" + vji2.Email + "','" + vji2.EmailStandardizedSuccess + "','" + vji2.EmailStandardized + "','" + vji2.Phone + "','" + vji2.PhoneStandardizedSuccess + "','" + vji2.PhoneStandardized + "','" + vji2.PhoneNetwork + "','" + vji2.Confirm + "','" + vji2.PaymentStatus + "')");
            }
            Command.Append(string.Join(",", Rows));
            Command.Append(";");

            conn.Open();
            using (MySqlCommand myCmd = new MySqlCommand(Command.ToString(), conn))
            {
                myCmd.Connection = conn;
                myCmd.CommandTimeout = 300;
                myCmd.CommandType = CommandType.Text;
                myCmd.ExecuteNonQuery();
            }
            conn.Close();
        }

        public List<VietJetInfomation> LoadAllData()
        {
            List<VietJetInfomation> ListVietJetInfomation = new List<VietJetInfomation>();
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            conn.Open();
            string[] result = new string[0];
            string cmd = "SELECT id,flightcode,firstname,lastname,dateflight1,flight1,flightcode1,dateflight2,flight2,flightcode2,seats FROM VietJetInfomation";
            MySqlCommand cmd1 = new MySqlCommand();
            cmd1.CommandText = cmd;
            cmd1.Connection = conn;
            MySqlDataReader dr = cmd1.ExecuteReader();
            while (dr.Read())
            {
                VietJetInfomation vji = new VietJetInfomation();
                vji.Id = Convert.ToInt32(dr["id"].ToString());
                vji.CustomerIsCode = dr["flightcode"].ToString();
                vji.FirstName= dr["firstname"].ToString();
                vji.LastName = dr["lastname"].ToString();
                vji.DateFlight1t1 = dr["dateflight1"].ToString();
                vji.Flight1 = dr["flight1"].ToString();
                vji.Verify1 = dr["flightcode1"].ToString();
                vji.DateFlight2t1 = dr["dateflight2"].ToString();
                vji.Flight2 = dr["flight2"].ToString();
                vji.Verify2 = dr["flightcode2"].ToString();
                vji.Seats = dr["seats"].ToString();
                ListVietJetInfomation.Add(vji);
            }
            conn.Close();
            return ListVietJetInfomation;
        }
        public List<VietJetInfomation2> LoadAllDataVJ2()
        {
            List<VietJetInfomation2> ListVietJetInfomation2 = new List<VietJetInfomation2>();
            MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
            conn.Open();
            string[] result = new string[0];
            string cmd = "SELECT id,customeriscode,firstname,lastname,dateflight1,flightcode1,dateflight2,flightcode2,email,emailstandardizedsuccess,emailstandardized,phone,phonestandardizedsuccess,phonestandardized,phonenetwork,confirm,paymentstatus FROM VietJetInfomation2";
            MySqlCommand cmd1 = new MySqlCommand();
            cmd1.CommandText = cmd;
            cmd1.Connection = conn;
            MySqlDataReader dr = cmd1.ExecuteReader();
            while (dr.Read())
            {
                VietJetInfomation2 vji2 = new VietJetInfomation2();
                vji2.Id = Convert.ToInt32(dr["id"].ToString());
                vji2.CustomerIsCode = dr["customeriscode"].ToString();
                vji2.FirstName = dr["firstname"].ToString();
                vji2.LastName = dr["lastname"].ToString();
                vji2.DateFlight1t2 = dr["dateflight1"].ToString();
                vji2.FlightCode1 = dr["flightcode1"].ToString();
                vji2.DateFlight2t2 = dr["dateflight1"].ToString();
                vji2.FlightCode2 = dr["flightcode2"].ToString();
                vji2.Email = dr["email"].ToString();
                vji2.EmailStandardizedSuccess = dr["emailstandardizedsuccess"].ToString();
                vji2.EmailStandardized = dr["emailstandardized"].ToString();
                vji2.Phone = dr["phone"].ToString();
                vji2.PhoneStandardizedSuccess = dr["phonestandardizedsuccess"].ToString();
                vji2.PhoneStandardized = dr["phonestandardized"].ToString();
                vji2.PhoneNetwork = dr["phonenetwork"].ToString();
                vji2.Confirm = dr["confirm"].ToString();
                vji2.PaymentStatus = dr["paymentstatus"].ToString();
                ListVietJetInfomation2.Add(vji2);
            }
            conn.Close();
            return ListVietJetInfomation2;
        }

        //public string ReadIdMax()
        //{
        //    MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //    conn.Open();
        //    string result = null;
        //    string cmd = "SELECT id FROM VietJetInfomation WHERE id=(SELECT max(id) FROM VietJetInfomation)";
        //    MySqlCommand cmd1 = new MySqlCommand();
        //    cmd1.CommandText = cmd;
        //    cmd1.Connection = conn;
        //    MySqlDataReader dr = cmd1.ExecuteReader();
        //    while (dr.Read())
        //    {
        //        result = dr["id"].ToString();
        //        break;
        //    }
        //    conn.Close();
        //    return result;
        //}
        //public void Update(string id, string flightcode, string firstname, string lastname, string dateflight1, string flight1, string flightcode1, string dateflight2, string flight2, string flightcode2, string seats)
        //{
        //    MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //    conn.Open();
        //    MySqlCommand cmd = new MySqlCommand();
        //    cmd.CommandText = "UPDATE VietJetInfomation SET flightcode='" + flightcode + "',firstname='" + firstname + "',lastname='" + lastname + "',dateflight1='" + dateflight1 + "',flight1='" + flight1 + "',flightcode1='" + flightcode1 + "',dateflight2='" + dateflight2 + "',flight2='" + flight2 + "',flightcode2='" + flightcode2 + "',seats='" + seats + "' WHERE id=" + id;
        //    cmd.Connection = conn;
        //    cmd.ExecuteNonQuery();
        //    conn.Close();
        //}
        //public bool DeleteID(string id)
        //{
        //    bool success = false;
        //    try
        //    {
        //        MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //        conn.Open();
        //        MySqlCommand cmd = new MySqlCommand();
        //        cmd.CommandText = "DELETE FROM VietJetInfomation WHERE id=" + id;
        //        cmd.Connection = conn;
        //        cmd.ExecuteNonQuery();
        //        conn.Close();
        //        success = true;
        //    }
        //    catch { }
        //    return success;
        //}
        //public VietJetInfomation GetVietJetInfomation(string id)
        //{
        //    VietJetInfomation vji = new VietJetInfomation();
        //    MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //    conn.Open();
        //    string[] result = new string[0];
        //    string cmd = "SELECT flightcode,firstname,lastname,dateflight1,flight1,flightcode1,dateflight2,flight2,flightcode2,seats FROM VietJetInfomation WHERE id=" + id;
        //    MySqlCommand cmd1 = new MySqlCommand();
        //    cmd1.CommandText = cmd;
        //    cmd1.Connection = conn;
        //    MySqlDataReader dr = cmd1.ExecuteReader();
        //    while (dr.Read())
        //    {
        //        vji.Id = Convert.ToInt32(id);
        //        vji.CustomerIsCode = dr["flightcode"].ToString();
        //        vji.FirstName = dr["firstname"].ToString();
        //        vji.DateFlight1 = dr["dateflight1"].ToString();
        //        vji.Flight1 = dr["flight1"].ToString();
        //        vji.Verify1 = dr["flightcode1"].ToString();
        //        vji.DateFlight2 = dr["dateflight2"].ToString();
        //        vji.Flight2 = dr["flight2"].ToString();
        //        vji.Verify2 = dr["flightcode2"].ToString();
        //        vji.Seats = dr["seats"].ToString();
        //        break;
        //    }
        //    conn.Close();
        //    return vji;
        //}
        //public VietJetInfomation2 GetVietJetInfomation2(string customeriscode)
        //{
        //    VietJetInfomation2 vji2 = new VietJetInfomation2();
        //    MySqlConnection conn = new MySqlConnection("SERVER=" + InputData.Server + ";DATABASE=" + InputData.Database + ";UID=" + InputData.UID + ";PASSWORD=" + InputData.Password + ";CHARSET=utf8;");
        //    conn.Open();
        //    //string[] result = new string[0];
        //    string cmd = "SELECT id,dateflight1,flightcode1,dateflight2,flightcode2,email,emailstandizedsuccess,emailstandized,phone,phonestandizedsuccess,phonestandized,confirm,paymentstatus FROM VietJetInfomation WHERE customeriscode=" + customeriscode;
        //    MySqlCommand cmd1 = new MySqlCommand();
        //    cmd1.CommandText = cmd;
        //    cmd1.Connection = conn;
        //    MySqlDataReader dr = cmd1.ExecuteReader();
        //    while (dr.Read())
        //    {
        //        vji2.Id = Convert.ToInt32(dr["id"].ToString());
        //        vji2.CustomerIsCode = customeriscode;
        //        vji2.DateFlight1 = dr["dateflight1"].ToString();
        //        vji2.FlightCode1 = dr["flightcode1"].ToString();
        //        vji2.DateFlight2 = dr["dateflight2"].ToString();
        //        vji2.FlightCode2 = dr["flightcode2"].ToString();
        //        vji2.Email = dr["email"].ToString();
        //        vji2.EmailStandardizedSuccess = dr["emailstandizedsuccess"].ToString();
        //        vji2.EmailStandardized = dr["emailstandized"].ToString();
        //        vji2.Phone = dr["phone"].ToString();
        //        vji2.PhoneStandardizedSuccess = dr["phonestandizedsuccess"].ToString();
        //        vji2.PhoneStandardized = dr["phonestandized"].ToString();
        //        vji2.PhoneNetwork = dr["phonenetwork"].ToString();
        //        vji2.Confirm= dr["confirm"].ToString();
        //        vji2.PaymentStatus= dr["paymentstatus"].ToString();
        //        break;
        //    }
        //    conn.Close();
        //    return vji2;
        //}


    }
    class VietJetInfomation
    {
        public int Id;
        public string CustomerIsCode;
        public string FirstName;
        public string LastName;
        public string DateFlight1t1;
        public string Flight1;
        public string Verify1;
        public string DateFlight2t1;
        public string Flight2;
        public string Verify2;
        public string Seats;
        public string DateFlight1t2;
        public string FlightCode1;
        public string DateFlight2t2;
        public string FlightCode2;
        public string Email;
        public string EmailStandardizedSuccess;
        public string EmailStandardized;
        public string Phone;
        public string PhoneStandardizedSuccess;
        public string PhoneStandardized;
        public string PhoneNetwork;
        public string Confirm;
        public string PaymentStatus;

    }
    class VietJetInfomation1
    {
        public string CustomerIsCode;
        public string FirstName;
        public string LastName;
        public string DateFlight1;
        public string Flight1;
        public string FlightCode1;
        public string DateFlight2;
        public string Flight2;
        public string FlightCode2;
        public string Seats;
    }
    class VietJetInfomation2
    {
        public int Id;
        public string CustomerIsCode;
        public string FirstName;
        public string LastName;
        public string DateFlight1t2;
        public string FlightCode1;
        public string DateFlight2t2;
        public string FlightCode2;
        public string Email;
        public string EmailStandardizedSuccess;
        public string EmailStandardized;
        public string Phone;
        public string PhoneStandardizedSuccess;
        public string PhoneStandardized;
        public string PhoneNetwork;
        public string Confirm;
        public string PaymentStatus;
    }
}
