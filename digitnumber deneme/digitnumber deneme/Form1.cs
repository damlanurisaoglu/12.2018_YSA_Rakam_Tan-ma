using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace digitnumber_deneme
{
    public partial class Form1 : Form
    {
        int araKatmanSayisi = 24;
        double[,] x = new double[1195,266];
        double[,] xTest = new double[398, 266];

        double[,] xTestSonuc = new double[2, 398];//Değiştirme
        double[,] w_ = new double[24, 256];
        double[,] w = new double[1, 24];
        double[,] b_ = new double[1, 24];
        double[] b = new double[1];
        double[,] oP_ = new double[1, 24];
        double[] oP = new double[1];
        double n = 0.8;//önceki değeri 0.5     0.8
        double alfa = 0.9;//önceki değeri 0.8    0.9
        double[,] hFw_ = new double[24, 256];
        double[,] hFw = new double[1, 24];
        double[,] hFb_ = new double[1, 24];
        double[] hFb = new double[1];
        double t;//çıkış değeri 1195
        public int iterasyonSayisi;
        int epochSayisi;
        int güncellenmeyenIterasyonSayisi;
        int testIterasyonSayisi;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

           
            string[] kayıt_girdisi = new string[1195]; int i = 0;
            StreamReader oku = new StreamReader("deneme.egitim2.txt");         
            while (!oku.EndOfStream)
            {
                kayıt_girdisi[i] = oku.ReadLine();
                string[] veriler = kayıt_girdisi[i].Split(' ');

                int k = 0;
                for (int j = 0; j < 266; j++)
                {
                    x[i, j] = double.Parse(veriler[k].Substring(0, 1).ToString());
                    k++;
                }
                
                i++;
            }


            string[] kayıt_girdisi2 = new string[398]; int y = 0;
            StreamReader oku2 = new StreamReader("deneme.test2.txt");
            while (!oku2.EndOfStream)
            {
                kayıt_girdisi2[y] = oku2.ReadLine();
                string[] veriler2 = kayıt_girdisi2[y].Split(' ');

                int k = 0;
                for (int j = 0; j < 266; j++)
                {
                    xTest[y, j] = double.Parse(veriler2[k].Substring(0, 1).ToString());
                    k++;
                }

                y++;
            }
            Random rnd = new Random();
            for (int z = 0; z < 256; z++)
            {
                for (int t = 0; t < araKatmanSayisi; t++)
                {
                    w_[t, z] = rnd.NextDouble()*((1)-(-1))+(-1);
                }
            }
            for (i = 0; i < araKatmanSayisi; i++)
            {
                w[0, i] = rnd.NextDouble() * ((1) - (-1)) + (-1);
                b_[0, i] = rnd.NextDouble() * ((1) - (-1)) + (-1);
            }
            b[0] = rnd.NextDouble() * ((1) - (-1)) + (-1);
            iterasyonSayisi = 0;
            epochSayisi = 1;
            güncellenmeyenIterasyonSayisi = 0;
            testIterasyonSayisi = 0;
            for (int r = 0; r < araKatmanSayisi; r++)
            {
                hFw[0, r] = 0;
            }
            

        }

        private void button1_Click(object sender, EventArgs e)//EĞİTİMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM
        {
            button1.Visible = false;
            //while (güncellenmeyenIterasyonSayisi<=1195)
            //while (epochSayisi <= 5000)
            while (epochSayisi <= 5000)
            {

                for (int i = 0; i < araKatmanSayisi; i++)
                {
                    oP_[0, i] = sigmoidHesapla(aracikisBul(x, w_, b_, i, iterasyonSayisi));//8 gizli nöron çıkışını buluyor.
                }
                oP[0] = sigmoidHesapla(soncikisBul(oP_, w, b));//son nöronun çıkışını buluyor.
                double k = 0;
                for (int i = 256; i < 266; i++)//son 10 veriye bakarak t'nin ne olduğunu buluyor.
                {

                    if (x[iterasyonSayisi, i] == 1)
                    {
                        t = k;
                    }
                    else
                    {
                        k++;
                    }

                }

                //   if (oP[0]!=t) son nöronun çıkış değeri t 'ye  eşit değil ise hata fonk. ve yeni değerler hesaplanıyor.

                //{
                /*son nöron ağılık ve bias delta hesaplamaları BAŞLANGIÇ.*/
                double hataoP = hataFonk(t / 10, oP[0]);
                //Math.Abs(hataoP) >= 0.004           hata>= 0.049
                //double hata = Math.Abs(oP[0] - (t / 10));
                if (Math.Abs(hataoP) >= 0.01)
                {
                    for (int i = 0; i < araKatmanSayisi; i++)
                    {
                        hFw[0, i] = deltaW(hataoP, oP_[0, i], hFw[0, i], n, alfa);//son nörona giden ağırlıkların deltası hesaplanıyor.
                                                                                  // double deltaW = (n * hata * oP) + (alfa * hataFarki);
                    }


                    hFb[0] = deltaB(hataoP, hFb[0], n, alfa);//son katmanın delta bias'ı hesaplanıyor.
                                                             /*son nöron ağılık ve bias delta hesaplamaları BİTİŞ.*/


                    /*ara nöronlar ağılık ve bias delta hesaplamaları BAŞLANGIÇ.*/
                    double[] hataoP_ = new double[araKatmanSayisi];//ara nöronların hata fonksiyonları dizide tutuluyor (8 tane)
                    for (int i = 0; i < araKatmanSayisi; i++)
                    {
                        hataoP_[i] = hataFonk_(oP_[0, i], hataoP, w[0, i]);//sırayla hata fonk. hesaplanıyor.
                    }

                    for (int j = 0; j < araKatmanSayisi; j++)
                    {
                        for (int i = 0; i < 256; i++)
                        {
                            hFw_[j, i] = deltaW(hataoP_[j], x[iterasyonSayisi, i], hFw_[j, i], n, alfa);
                            //  double deltaW = (n * hata * x) + (alfa * hataFarki);
                            hFb_[0, j] = deltaB(hataoP_[j], hFb_[0, j], n, alfa);//deltab =n * hata + alfa * hFb
                        }

                    }
                    /*ara nöronlar ağılık ve bias delta hesaplamaları BİTİŞ.*/

                    for (int i = 0; i < araKatmanSayisi; i++)//ara nöronların ağırlıklarının (w) hesaplanması.
                    {
                        w[0, i] = w[0, i] + hFw[0, i];
                    }

                    b[0] = b[0] + hFb[0];//son nöronun bias hesaplaması

                    for (int i = 0; i < araKatmanSayisi; i++)//giriş değerlerin ağırlıklarının (w_) hesaplanması
                    {
                        for (int j = 0; j < 256; j++)
                        {
                            w_[i, j] = w_[i, j] + hFw_[i, j];
                        }
                    }
                    for (int i = 0; i < araKatmanSayisi; i++)//ara nöronların bias hesaplamaları
                    {
                        b_[0, i] = b_[0, i] + hFb_[0, i];
                    }
                    güncellenmeyenIterasyonSayisi = 0;
                   

                }
                else//güncellenme yoksa
                {
                    güncellenmeyenIterasyonSayisi++;
                }
                iterasyonSayisi++;
                if (iterasyonSayisi == 1195)
                {
                    epochSayisi++;
                    iterasyonSayisi = 0;
                }

                //if (güncellenmeyenIterasyonSayisi == 1195)//1195 tane iterasyon güncellenmeden kontrol edildi demek.
                //{
                //    MessageBox.Show("Güncellemeler tamamlandı!!!         "+DateTime.Now.Hour + ":" + DateTime.Now.Minute );
                //}

                //if (epochSayisi == 5000)//1195 tane iterasyon güncellenmeden kontrol edildi demek.
                //{
                //    MessageBox.Show("Güncellemeler tamamlandı!!!         " + DateTime.Now.Hour + ":" + DateTime.Now.Minute);
                //    epochSayisi++;
                //}
                

            }

            MessageBox.Show(DateTime.Now.Hour + ":" + DateTime.Now.Minute + "    Epoch Sayisi:" + epochSayisi);
            //kaydet();
            button1.Visible = true;
        }


        private void button2_Click(object sender, EventArgs e)
        {

            //string[] kayıt_girdisi = new string[12]; int s = 0;
            //StreamReader oku = new StreamReader("w_.txt");
            //while (!oku.EndOfStream)
            //{
            //    kayıt_girdisi[s] = oku.ReadLine();
            //    string[] veriler = kayıt_girdisi[s].Split('\t');

            //    int k = 0;
            //    for (int j = 0; j < 256; j++)
            //    {
            //        w_[s, j] = double.Parse(veriler[k].ToString());
            //        k++;
            //    }

            //    s++;
            //}
            //string[] kayıt_girdisi2 = new string[1]; s = 0;
            //StreamReader oku2 = new StreamReader("w.txt");
            //while (!oku2.EndOfStream)
            //{
            //    kayıt_girdisi2[s] = oku2.ReadLine();
            //    string[] veriler2 = kayıt_girdisi2[s].Split('\t');

            //    int k = 0;
            //    for (int j = 0; j < 12; j++)
            //    {
            //        w[s, j] = double.Parse(veriler2[k].ToString());
            //        k++;
            //    }

            //    s++;
            //}
            //string[] kayıt_girdisi3 = new string[1]; s = 0;
            //StreamReader oku3 = new StreamReader("b_.txt");
            //while (!oku3.EndOfStream)
            //{
            //    kayıt_girdisi3[s] = oku3.ReadLine();
            //    string[] veriler3 = kayıt_girdisi3[s].Split('\t');

            //    int k = 0;
            //    for (int j = 0; j < 12; j++)
            //    {
            //        b_[s, j] = double.Parse(veriler3[k].ToString());
            //        k++;
            //    }

            //    s++;
            //}
            //string[] kayıt_girdisi4 = new string[1]; s = 0;
            //StreamReader oku4 = new StreamReader("b.txt");
            //while (!oku4.EndOfStream)
            //{
            //    kayıt_girdisi4[s] = oku4.ReadLine();
            //    string[] veriler4 = kayıt_girdisi4[s].Split('\t');

            //    int k = 0;

            //    b[s] = double.Parse(veriler4[k].ToString());
            //    k++;


            //    s++;
            //}
            int[] dogruTahmin = new int[10];
            int[] yanlisTahmin = new int[10];
            int tDogruTahmin = 0;
            int tYanlisTahmin = 0;
            for (int i = 0; i < 10; i++)
            {
                dogruTahmin[i] = 0;
            }
            for (int i = 0; i < 10; i++)
            {
                yanlisTahmin[i] = 0;
            }
            while (testIterasyonSayisi <= 397)
            {
                for (int i = 0; i < araKatmanSayisi; i++)
                {
                    oP_[0, i] = sigmoidHesapla(aracikisBul(xTest, w_, b_, i, testIterasyonSayisi));//8 gizli nöron çıkışını buluyor.
                }
                oP[0] = sigmoidHesapla(soncikisBul(oP_, w, b));//son nöronun çıkışını buluyor.
                double k = 0;
                for (int i = 256; i < 266; i++)//son 10 veriye bakarak t'nin ne olduğunu buluyor.
                {

                    if (xTest[testIterasyonSayisi, i] == 1)
                    {
                        t = k;
                        break;
                    }

                    else
                    {
                        k++;
                        
                    }

                }
                
                xTestSonuc[0, testIterasyonSayisi] = oP[0]*10;
                xTestSonuc[1, testIterasyonSayisi] = t;
                if (Math.Abs(xTestSonuc[0, testIterasyonSayisi] - xTestSonuc[1, testIterasyonSayisi] )<=0.4)
                {
                    dogruTahmin[int.Parse( t.ToString())] = dogruTahmin[int.Parse(t.ToString())] + 1;
                    tDogruTahmin++;
                }
                else
                {
                    yanlisTahmin[int.Parse(t.ToString())] = yanlisTahmin[int.Parse(t.ToString())] + 1;
                    tYanlisTahmin++;
                }
                testIterasyonSayisi++;


            }

        }
        public static double aracikisBul(double[,] x, double[,] w_, double[,] b_, int k,int iterasyoSay)
        {
            double toplam = 0;
              
                for (int j = 0; j < 256; j++)
                {
                    toplam = toplam + x[iterasyoSay, j] * w_[k, j];
                }
                toplam = toplam + b_[0, k];

            

            return toplam;
        }
        public double soncikisBul(double[,] oP_, double[,] w, double[] b)
        {
            double toplam = 0;

            for (int j = 0; j < araKatmanSayisi; j++)
            {
                toplam = toplam + oP_[0, j] * w[0, j];
            }
            toplam = toplam + b[0];



            return toplam;
        }
        public static double sigmoidHesapla(double sayi)
        {
            double sonuc = 1 / (1 + (Math.Exp(-1 * sayi)));

            return sonuc;
        }
        public static double hataFonk(double gercek, double bulunan)
        {
            double hata = (gercek - bulunan) * (bulunan) * (1 - bulunan);

            return hata;
        }
        public static double deltaW(double hata, double oP, double hataFarki, double n, double alfa)
        {
            double deltaW = (n * hata * oP) + (alfa * hataFarki);
            return deltaW;
        }
        public static double deltaB(double hata, double hataFarki, double n, double alfa)
        {
            double deltaB = (n * hata) + (alfa * hataFarki);
            return deltaB;
        }
        public static double hataFonk_(double bulunan, double hataP1, double agirlik)
        {
            double hata = bulunan * (1 - bulunan) * hataP1 * agirlik;
            return hata;
        }
        public void kaydet()
        {
            Microsoft.Office.Interop.Excel.Application objexcelappW_ = new Microsoft.Office.Interop.Excel.Application();
            objexcelappW_.Application.Workbooks.Add(Type.Missing);
            objexcelappW_.Columns.ColumnWidth = 4;


            for (int i = 0; i < araKatmanSayisi; i++)
            {
                for (int j = 0; j < 256; j++)
                {

                    objexcelappW_.Cells[i + 1, j + 1] = w_[i, j];

                }
            }
            MessageBox.Show("Çıktı Başarıyla Alındı C:\\YEDEK/w_.xlsx");
            objexcelappW_.ActiveWorkbook.SaveCopyAs("C:\\YEDEK/w_.xlsx");
            objexcelappW_.ActiveWorkbook.Saved = true;


            Microsoft.Office.Interop.Excel.Application objexcelappW = new Microsoft.Office.Interop.Excel.Application();
            objexcelappW.Application.Workbooks.Add(Type.Missing);
            objexcelappW.Columns.ColumnWidth = 4;


            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < araKatmanSayisi; j++)
                {

                    objexcelappW.Cells[i + 1, j + 1] = w[i, j];

                }
            }
            MessageBox.Show("Çıktı Başarıyla Alındı C:\\YEDEK/w.xlsx");
            objexcelappW.ActiveWorkbook.SaveCopyAs("C:\\YEDEK/w.xlsx");
            objexcelappW.ActiveWorkbook.Saved = true;


            Microsoft.Office.Interop.Excel.Application objexcelappB_ = new Microsoft.Office.Interop.Excel.Application();
            objexcelappB_.Application.Workbooks.Add(Type.Missing);
            objexcelappB_.Columns.ColumnWidth = 4;


            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < araKatmanSayisi; j++)
                {

                    objexcelappB_.Cells[i + 1, j + 1] = b_[i, j];

                }
            }
            MessageBox.Show("Çıktı Başarıyla Alındı C:\\YEDEK/b_.xlsx");
            objexcelappB_.ActiveWorkbook.SaveCopyAs("C:\\YEDEK/b_.xlsx");
            objexcelappB_.ActiveWorkbook.Saved = true;



            Microsoft.Office.Interop.Excel.Application objexcelappB = new Microsoft.Office.Interop.Excel.Application();
            objexcelappB.Application.Workbooks.Add(Type.Missing);
            objexcelappB.Columns.ColumnWidth = 4;


            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < 1; j++)
                {

                    objexcelappB.Cells[i + 1, j + 1] = b[i];

                }
            }
            MessageBox.Show("Çıktı Başarıyla Alındı C:\\YEDEK/b.xlsx");
            objexcelappB.ActiveWorkbook.SaveCopyAs("C:\\YEDEK/b.xlsx");
            objexcelappB.ActiveWorkbook.Saved = true;
        }
       
    }
}
