using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data;
using System.Data.SqlClient;

namespace RFID
{
 
  

    public partial class Form1 : Form
    {
        private String sql_select = "";
        private SqlCommand command;
        private SqlDataReader datareader;
        private int port = 3;
        private Database db = new Database();


        private void label1_Click(object sender, EventArgs e)
        {

        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public Form1()
        {
            InitializeComponent();
        }

        private void frmRFIDReader_Load(object sender, EventArgs e)
        {

        }


        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern void Sleep(int dwMilliseconds);
        [DllImport("MasterRD.dll")]
        static extern int lib_ver(ref uint pVer);
        [DllImport("MasterRD.dll")]
        static extern int rf_init_com(int port, int baud);
        [DllImport("MasterRD.dll")]
        static extern int rf_ClosePort();
        [DllImport("MasterRD.dll")]
        static extern int rf_antenna_sta(short icdev, byte mode);
        [DllImport("MasterRD.dll")]
        static extern int rf_init_type(short icdev, byte type);
        [DllImport("MasterRD.dll")]
        static extern int rf_request(short icdev, byte mode, ref ushort pTagType);
        [DllImport("MasterRD.dll")]
        static extern int rf_anticoll(short icdev, byte bcnt, IntPtr pSnr, ref byte pRLength);
        [DllImport("MasterRD.dll")]
        static extern int rf_select(short icdev, IntPtr pSnr, byte srcLen, ref sbyte Size);
        [DllImport("MasterRD.dll")]
        static extern int rf_halt(short icdev);
        [DllImport("MasterRD.dll")]
        static extern int rf_M1_authentication2(short icdev, byte mode, byte secnr, IntPtr key);
        [DllImport("MasterRD.dll")]
        static extern int rf_M1_initval(short icdev, byte adr, Int32 value);
        [DllImport("MasterRD.dll")]
        static extern int rf_M1_increment(short icdev, byte adr, Int32 value);
        [DllImport("MasterRD.dll")]
        static extern int rf_M1_decrement(short icdev, byte adr, Int32 value);
        [DllImport("MasterRD.dll")]
        static extern int rf_M1_readval(short icdev, byte adr, ref Int32 pValue);
        [DllImport("MasterRD.dll")]
        static extern int rf_M1_read(short icdev, byte adr, IntPtr pData, ref byte pLen);
        [DllImport("MasterRD.dll")]
        static extern int rf_M1_write(short icdev, byte adr, IntPtr pData);
        bool bConnectedDevice;
        static char[] hexDigits = {
'0','1','2','3','4','5','6','7',
'8','9','A','B','C','D','E','F'};
        public static byte GetHexBitsValue(byte ch)
        {
            byte sz = 0;
            if (ch <= '9' && ch >= '0')
                sz = (byte)(ch - 0x30);
            if (ch <= 'F' && ch >= 'A')
                sz = (byte)(ch - 0x37);
            if (ch <= 'f' && ch >= 'a')
                sz = (byte)(ch - 0x57);
            return sz;
        }
        public static string ToHexString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }
        public static byte[] ToDigitsBytes(string theHex)
        {
            byte[] bytes = new byte[theHex.Length / 2 + (((theHex.Length % 2) > 0) ? 1 : 0)];
            for (int i = 0; i < bytes.Length; i++)
            {
                char lowbits = theHex[i * 2];
                char highbits;
                if ((i * 2 + 1) < theHex.Length)
                    highbits = theHex[i * 2 + 1];
                else
                    highbits = '0';
                int a = (int)GetHexBitsValue((byte)lowbits);
                int b = (int)GetHexBitsValue((byte)highbits);
                bytes[i] = (byte)((a << 4) + b);
            }
            return bytes;
        }








        private void re1()
        {
            short icdev = 0x0000;
            int status;
            byte type = (byte)'A';//mifare one 卡询卡方式为A
            byte mode = 0x52;
            ushort TagType = 0;
            byte bcnt = 0x04;//mifare 卡都用4
            IntPtr pSnr;
            byte len = 255;
            sbyte size = 0;
            if (!bConnectedDevice)
            {
                // MessageBox.Show("Not connect to device!!", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                label1.Text = "Not connect to device!!";
                return;
            }
            pSnr = Marshal.AllocHGlobal(1024);
            for (int i = 0; i < 2; i++)
            {
                status = rf_antenna_sta(icdev, 0);//关闭天线
                if (status != 0)
                    continue;
                Sleep(20);
                status = rf_init_type(icdev, type);
                if (status != 0)
                    continue;
                Sleep(20);
                status = rf_antenna_sta(icdev, 1);//启动天线
                if (status != 0)
                    continue;
                Sleep(50);
                status = rf_request(icdev, mode, ref TagType);//搜寻所有的卡
                if (status != 0)
                    continue;
                status = rf_anticoll(icdev, bcnt, pSnr, ref len);//返回卡的序列号
                if (status != 0)
                    continue;
                status = rf_select(icdev, pSnr, len, ref size);//锁定一张ISO14443-3 TYPE_A 卡
                if (status != 0)
                    continue;
                byte[] szBytes = new byte[len + 1];
                string str = Marshal.PtrToStringAnsi(pSnr);
                for (int j = 0; j < len; j++)
                {
                    szBytes[j] = (byte)str[j];
                }
                //textBox2.Text = ToHexString(szBytes);
                break;
            }
            Marshal.FreeHGlobal(pSnr);
        }
        private void ReadTagData()
        {
            short icdev = 0x0000;
            int status;
            byte mode = 0x60;
            byte secnr = 0x00;
            string a1, a2, a3;
            String x1 = "", x2 = "", x3 = "", x4 = "", x5 = "", x6 = "", x7 = "", x8 = "", x9 = "";
            if (!bConnectedDevice)
            {
                label1.Text = "Not connect to device!!";
                return;
            }
            secnr = 2;// Convert.ToByte(cbxMass2.Text);
            IntPtr keyBuffer = Marshal.AllocHGlobal(256);
            byte[] bytesKey = ToDigitsBytes("AFDFEFDF1F6A");// (txtInputKey2.Text);
            for (int i = 0; i < bytesKey.Length; i++)
                Marshal.WriteByte(keyBuffer, i * Marshal.SizeOf(typeof(Byte)), bytesKey[i]);
            status = rf_M1_authentication2(icdev, mode, (byte)(secnr * 4), keyBuffer);
            Marshal.FreeHGlobal(keyBuffer);
            if (status != 0)
            {
                textBox1.Text = "";
               // textBox2.Text = "";
                return;
            }
            IntPtr dataBuffer = Marshal.AllocHGlobal(256);
            for (int i = 0; i < 4; i++)
            {
                int j;
                byte cLen = 0;
                status = rf_M1_read(icdev, (byte)((secnr * 4) + i), dataBuffer, ref cLen);
                if (status != 0 || cLen != 16)
                {
                    textBox1.Text = "";
                   // textBox2.Text = "";
                    Marshal.FreeHGlobal(dataBuffer);
                    return;
                }
                byte[] bytesData = new byte[16];
                for (j = 0; j < bytesData.Length; j++)
                    bytesData[j] = Marshal.ReadByte(dataBuffer, j);
                if (i == 0)
                    a1 = ToHexString(bytesData);// txtBoxDataOne2.Text = ToHexString(bytesData);
                else if (i == 1)
                    a2 = ToHexString(bytesData);// txtBoxDataTwo2.Text = ToHexString(bytesData);
                else if (i == 2)
                    textBox1.Text = ToHexString(bytesData).Substring(0, 13);// txtDataThree2.Text = ToHexString(bytesData);
                else if (i == 3)
                {
                    byte[] byteskeyA = new byte[6];
                    byte[] byteskey = new byte[4];
                    byte[] byteskeyB = new byte[6];
                    for (j = 0; j < 16; j++)
                    {
                        if (j < 6)
                            byteskeyA[j] = bytesData[j];
                        else if (j >= 6 && j < 10)
                            byteskey[j - 6] = bytesData[j];
                        else
                            byteskeyB[j - 10] = bytesData[j];
                    }

                    int x;
                    for (x = 0; x < 5; x++)
                    {//3660400256095
                        if (x == 0)
                        { x1 = textBox1.Text.Substring(0, 1); }
                        else if (x == 1)
                        { x2 = textBox1.Text.Substring(1, 4); }
                        else if (x == 2)
                        { x3 = textBox1.Text.Substring(5, 5); }
                        else if (x == 3)
                        { x4 = textBox1.Text.Substring(10, 2); }
                        else if (x == 4)
                        {
                            x5 = textBox1.Text.Substring(12, 1);
                        }
                    }
                    x6 = x1 + "-" + x2 + "-" + x3 + "-" + x4 + "-" + x5;
                    textBox1.Text = x6;
                    addData(textBox1.Text);
                }
            }
            Marshal.FreeHGlobal(dataBuffer);
        }





        private void button1_Click(object sender, EventArgs e)
        {
            int baud = 0;
            int status;
            port = 4;// Convert.ToInt32(tscbxPort.Text);
            baud = 9600;// Convert.ToInt32(tscbxBaud.Text);
            status = rf_init_com(port, baud);
            if (0 == status)
            {
                bConnectedDevice = true;
                textBox1.Text = "Ok";
            }
            else
            {
                bConnectedDevice = false;
                textBox1.Text = "false";
            }
        }


        private void timer1_Tick_1(object sender, EventArgs e)
        {
            re1();
            ReadTagData();

        }

        private void button2_Click(object sender, EventArgs e)
        {

            SqlConnection con = db.connection();
            String sql_insert = "INSERT INTO customer (Id, name, sername) VALUES ('4','xx','xx')";

            try
            {
                con.Open();
                command = new SqlCommand(sql_insert, con);
                datareader = command.ExecuteReader();
                MessageBox.Show("insert ok");
            }
            catch (SqlException)
            {
                MessageBox.Show("Error Sql");
            }
            catch (Exception ex)
            {
                MessageBox.Show("connect Error");
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
           
            SqlConnection con = db.connection();
            sql_select = "select * from customer";
            try
            {
                con.Open();
                command = new SqlCommand(sql_select, con);
                datareader = command.ExecuteReader();
                while (datareader.Read())
                {
                    MessageBox.Show(datareader.GetValue(0).ToString() + datareader.GetValue(1).ToString() + datareader.GetValue(2).ToString());
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("connect Error");
            }

        }

        private void addData(String id)
        {

           
            SqlConnection con = db.connection();

            String sql_insert = "INSERT INTO customer (Id, name, sername) VALUES ('"+id+"','xx','xx')";
            try
            {
                con.Open();
                command = new SqlCommand(sql_insert, con);
                MessageBox.Show("insert ok");
            }
            catch (Exception ex)
            {
                MessageBox.Show("insert Error");
            }
        }
    }

}
