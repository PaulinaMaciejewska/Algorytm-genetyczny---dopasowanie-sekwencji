using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Timers;

namespace ProjektZP
{
    public partial class Form1 : Form
    {
        System.Timers.Timer t;
        int godz, min, sek;
        Random r = new Random();

        FileStream fs, fsi, fo, fsa;
        static int m, n, d, liczba_testow;
        char[,] sekwencja_tab;
        List<string> lista = new List<string>();
        List<List<int>> populacja = new List<List<int>>();
        List<List<int>> rodzice = new List<List<int>>();
        List<List<int>> kolejne_pokolenie = new List<List<int>>();
        List<List<char>> najlepsze_dopasowanie = new List<List<char>>();
        List<int> Para = new List<int>();
        List<int> najlepszy_osobnik = new List<int>();

        int najlepsza_funkcja_celu;
        double sekundy;

        volatile bool Active = false;
        volatile bool Stop = false;
        volatile bool IsAlreadyRunning = false;

        public Form1()
        {
            InitializeComponent();
        }

        #region Przycisk zapisu
        private void ZapiszDoPliku_Click(object sender, EventArgs e)
        {
            if (LiczbaSekwencji.Text == "")
            {
                MessageBox.Show(@"Pole 'Liczba sekwencji' nie może być puste!", "Uwaga", MessageBoxButtons.OK);
            }
            if (DługośćSekwencji.Text == "")
            {
                MessageBox.Show(@"Pole 'Długość sekwencji' nie może być puste!", "Uwaga", MessageBoxButtons.OK);
            }
            if (DługośćDopasowania.Text == "")
            {
                MessageBox.Show(@"Pole 'Długość dopasowania' nie może być puste!", "Uwaga", MessageBoxButtons.OK);
            }
            if (Liczba_macierzy.Text == "")
            {
                MessageBox.Show(@"Pole 'Liczba wygenerowanych macierzy' nie może być puste!", "Uwaga", MessageBoxButtons.OK);
            }

            m = Convert.ToInt32(LiczbaSekwencji.Text);
            n = Convert.ToInt32(DługośćSekwencji.Text);
            d = Convert.ToInt32(DługośćDopasowania.Text);
            liczba_testow = Convert.ToInt32(Liczba_macierzy.Text);
            sekwencja_tab = new char[m, n];
            int ilość_błędów = Convert.ToInt32(IlośćBłędów.Text);
            int cz_A = Convert.ToInt32(częstośćA.Text) * d / 100;
            int cz_T = Convert.ToInt32(częstośćT.Text) * d / 100;
            int cz_C = Convert.ToInt32(częstośćC.Text) * d / 100;
            int cz_G = d - cz_A - cz_C - cz_T;

            if (częstośćA.Text == null || częstośćT.Text == null || częstośćC.Text == null || częstośćG.Text == null)
            {
                MessageBox.Show("Wszystkie pola muszą być wypełnione!", "Uwaga", MessageBoxButtons.OK);

            }
            if (d < n)
            {
                MessageBox.Show("Długość dopasowania musi być dłuższa od długości sekwencji!", "Uwaga", MessageBoxButtons.OK);
            }

            if ((Convert.ToInt32(częstośćA.Text) + Convert.ToInt32(częstośćT.Text) + Convert.ToInt32(częstośćC.Text) + Convert.ToInt32(częstośćG.Text)) != 100)
            {
                MessageBox.Show("Suma stosunków nukleotydów musi wynosić 100%!", "Uwaga", MessageBoxButtons.OK);
            }

            for (int test = 0; test < liczba_testow; test++)
            {
                int[,] macierz = new int[m, d];
                char[] sekwencja = Sekwencja_stosunki(cz_A, cz_T, cz_C, cz_G);
                Stwórz_zapisz_macierz(macierz, sekwencja, test, m, n, d);
                Zamien_na_sekwencje(macierz, sekwencja, m, d);
                Wprowadz_bledy(ilość_błędów, sekwencja_tab, m, n);
                Zapisz_sekwencje_macierz(sekwencja_tab, test, m, n);

            }
            MessageBox.Show("Sekwencje zostały zapisane w pliku!", "Uwaga", MessageBoxButtons.OK);
        }
        #endregion

        #region Ustalenie stosunków nukleotydów
        private static char[] Sekwencja_stosunki(int cz_A, int cz_T, int cz_C, int cz_G)
        {
            Random r = new Random();
            List<char> sekwencja1 = new List<char>();
            for (int i = 0; i < cz_A; i++)
            {
                sekwencja1.Add('A');
            }
            for (int j = 0; j < cz_T; j++)
            {
                sekwencja1.Add('T');
            }
            for (int k = 0; k < cz_C; k++)
            {
                sekwencja1.Add('C');
            }
            for (int l = 0; l < cz_G; l++)
            {
                sekwencja1.Add('G');
            }

            var sekwencja_shuffle = sekwencja1.OrderBy(item => r.Next());
            List<char> sekwencja_new = new List<char>();
            foreach (var znak in sekwencja_shuffle)
            {
                sekwencja_new.Add(znak);
            }
            char[] sekwencja = sekwencja_new.ToArray();
            return (sekwencja);
        }

        #endregion

        #region Stworzenie macierzy i zamiana na sekwencje
        private void Stwórz_zapisz_macierz(int[,] macierz, char[] sekwencja, int test, int m, int n, int d)
        {
            Random r = new Random();

            string ścieżka1 = Directory.GetCurrentDirectory() + "\\idealne"
                                + m.ToString() + "x" + n.ToString() + " - " + test.ToString() + ".txt";
            fsi = new FileStream(ścieżka1, FileMode.Create, FileAccess.Write);
            StreamWriter swi = new StreamWriter(fsi);

            swi.WriteLine(sekwencja);

            for (int i = 0; i < m; i++)
            {
                int Counter = 0;
                while (Counter < n)
                {
                    int index = r.Next(d);
                    if (macierz[i, index] == 0)
                    {
                        macierz[i, index] = 1;
                        Counter++;
                    }
                }
            }
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < d; j++)
                {
                    swi.Write(macierz[i, j]);
                }
                swi.WriteLine();
            }
            swi.Close();
            fsi.Close();
        }

        private void Zamien_na_sekwencje(int[,] macierz, char[] sekwencja, int m, int d)
        {
            for (int i = 0; i < m; i++)
            {
                int index = 0;
                for (int j = 0; j < d; j++)
                {
                    if (macierz[i, j] == 1)
                    {
                        sekwencja_tab[i, index] = sekwencja[j];
                        index++;
                    }
                    Console.WriteLine();
                }
            }
        }
        #endregion

        #region Wprowadzanie błędów
        private static char[,] Wprowadz_bledy(int ilość_błędów, char[,] sekwencja_tab, int m, int n)
        {
            Random r = new Random();

            int Counter = 0;
            while (Counter < ilość_błędów)
            {
                string zasady = "ACTG";
                int x = zasady.Length;
                int i = r.Next(m);
                int index = r.Next(n);
                if (sekwencja_tab[i, index] != zasady[r.Next(x)])
                {
                    sekwencja_tab[i, index] = zasady[r.Next(x)];
                    Counter++;
                }
                Console.Write(sekwencja_tab);
            }

            Console.WriteLine();
            return sekwencja_tab;
        }

        #endregion

        #region Zapis do pliku 
        private void Zapisz_sekwencje_macierz(char[,] sekwencja_tab, int test, int m, int n)
        {
            string ścieżka2 = Directory.GetCurrentDirectory() + "\\macierz" + m.ToString() + "x" + n.ToString() + " - " + test.ToString() + ".txt";
            fs = new FileStream(ścieżka2, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);

            for (int i = 0; i < m; i++)
            {
                for (int index = 0; index < n; index++)
                {
                    sw.Write(sekwencja_tab[i, index]);
                }
                sw.WriteLine();
            }

            sw.Close();
            fs.Close();
        }

        private void ZapiszDoPliku_Click_1(object sender, EventArgs e)
        {
            ZapiszDoPliku_Click(sender, e);

        }
        #endregion

        #region Wybór sekwencji z pliku i wysłanie do metaheurystyki
        private void WybierzZpliku_Click(object sender, EventArgs e)
        {
            OpenFileDialog plik = new OpenFileDialog();

            if (plik.ShowDialog() == DialogResult.OK)
            {
                string WybranaMacierz;
                WybranaMacierz = plik.FileName;
                fo = new FileStream(WybranaMacierz, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fo);

                WybraneSekwencje.Text = File.ReadAllText(plik.FileName);
            }
        }

        private void WyślijMacierz_Click(object sender, EventArgs e)
        {
            string[] seq = WybraneSekwencje.Text.Split('\n');
            if (seq.All(i => i.Length == seq[0].Length))
            {
                MessageBox.Show("Wszystkie sekwencje muszą być tej samej długości!", "Uwaga", MessageBoxButtons.OK);
            }

            List<char> nukleotydy = new List<char>() { 'A', 'C', 'T', 'G', 'a', 'c', 't', 'g','\n' };
            if (WybraneSekwencje.Text != "")
            {
                foreach (var linia in seq)
                {
                    bool błąd = false;
                    foreach (var znak in linia.Trim())
                    {
                        if (!nukleotydy.Contains(znak))
                        {
                            MessageBox.Show("Sekwencja może składać się tylko z liter: A,C T i G!", "Uwaga", MessageBoxButtons.OK);
                            błąd = true;
                            lista = null;
                            break;
                        }
                        else
                        {
                            błąd = false;
                            lista = seq.ToList();
                        }
                    }
                    if (lista == null)
                    {
                        break;
                    }
                }
            }
            if (lista != null)
            {
                for (int line = 0; line < lista.Count; line++)
                {
                    foreach (var r in lista[line])
                    {
                        if (r == '\r')
                        {
                            lista[line] = lista[line].TrimEnd('\r');
                        }
                    }
                    if (lista[line] == "")
                    {
                        lista.Remove(lista[line]);
                    }
                }
            }
            if (lista != null)
            {
                MessageBox.Show(@"Instancja została przesłana. Przejdź do zakładki 'Metaheurystyka'.", "Uwaga", MessageBoxButtons.OK);

            }
        }
        #endregion

        #region Start czasu i metaheurystyki / nowy wątek
        private void Form1_Load(object sender, EventArgs e)
        {
            t = new System.Timers.Timer
            {
                Interval = 1000
            };
            t.Elapsed += OnTimeEvent;
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            t.Start();

            if (!IsAlreadyRunning)
            {
                IsAlreadyRunning = true;

                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.DoWork += new DoWorkEventHandler
                    (
                    delegate (object o, DoWorkEventArgs args)
                    {
                        BackgroundWorker b = o as BackgroundWorker;
                        MetaheurystykaStart();
                        Active = true;
                        if (Active)
                        { }
                        if (Stop)
                        {
                            Stop = false;
                        }

                    }
                );

                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate (object o, RunWorkerCompletedEventArgs args)
                {
                    Funkcja_celu_i_dopasowanie();
                });

                bw.RunWorkerAsync();

                IsAlreadyRunning = false;
            }

        }

        private void btn_stop_Click(object sender, EventArgs e)
        {

            t.Stop();
            Stop = true;
            Active = false;
            Funkcja_celu_i_dopasowanie();
        }

        private void OnTimeEvent(object sender, ElapsedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                sek += 1;
                if (sek == 60)
                {
                    sek = 0;
                    min += 1;
                }
                if (min == 60)
                {
                    min = 0;
                    godz += 1;
                }
                od_startu.Text = string.Format("{0}:{1}:{2}", godz.ToString().PadLeft(2, '0'), min.ToString().PadLeft(2, '0'), sek.ToString().PadLeft(2, '0'));
            }));
        }
        #endregion
        private void RESET_Click(object sender, EventArgs e)
        {
            godz = 0;
            min = 0;
            sek = 0;
            od_startu.Text = string.Format("{0}:{1}:{2}", godz.ToString().PadLeft(2, '0'), min.ToString().PadLeft(2, '0'), sek.ToString().PadLeft(2, '0'));

            podgląd_dopasowania.Clear();
            podgląd_najlepszej_wartości_funkcji.Clear();
        }

        private bool if_stop_clicked()
        {
            if (Stop == true)
            {
                Stop = false;
                return true;
            }
            return false;
        }

        #region Elementy do tworzenia osobników i krzyżowania 
        private List<int> ListaNumerowSekwencji()
        {
            List<int> numerySekwencji = new List<int>();
            for (int i = 0; i < lista.Count; i++)
            {
                if (!numerySekwencji.Contains(i))
                {
                    numerySekwencji.Add(i);
                }
            }
            return numerySekwencji;
        }

        private Dictionary<int, int> NukleotydyDoWykorzystania()
        {

            Dictionary<int, int> DoWykorzystania = new Dictionary<int, int>();
            for (int i = 0; i < lista.Count; i++)
            {
                int z;
                if (lista[i].Contains('\r'))
                {
                    z = (lista[i].Length) - 1;
                }
                else
                {
                    z = lista[i].Length;
                }
                DoWykorzystania.Add(i, z);
            }
            return DoWykorzystania;
        }
        #endregion

        #region Tworzenie Osobnika i populacji początkowej
        private void TworzeniePopulacji(int wielkość_populacji, int m, int n)
        {
            populacja.Clear();
            for (int i = 0; i < wielkość_populacji; i++)
            {
                populacja.Add(StwórzOsobnika());
            }
        }

        private List<int> StwórzOsobnika()
        {
            Random r = new Random();
            var ListaSek = ListaNumerowSekwencji();
            var Słownik = NukleotydyDoWykorzystania();

            List<int> Osobnik = new List<int>();
            m = lista.Count;
            if (lista[0].Contains('\r'))
            {
                n = lista[0].Length - 1;
            }
            else
            {
                n = lista[0].Length;
            }

            while (ListaSek.Count != 0)
            {
                int WylosowanaSekwencja = ListaSek[r.Next(ListaSek.Count)];
                Osobnik.Add(WylosowanaSekwencja);
                char WylosowanyNukleotyd = lista[WylosowanaSekwencja][n - Słownik[WylosowanaSekwencja]];

                for (int i = 0; i < m; i++)
                {
                    if (Słownik[i] == 0)
                    {
                        continue;
                    }
                    if (lista[i][n - Słownik[i]] == WylosowanyNukleotyd)
                    {
                        Słownik[i]--;
                        if (Słownik[i] == 0)
                        {
                            ListaSek.Remove(i);
                        }
                    }
                }
            }
            return Osobnik;
        }
        #endregion

        #region WybórRodziców

        private void WybórRodziców(int wielkość_populacji)
        {
            rodzice.Clear();
            Para.Clear();

            int suma = 0;
            int funkcja_celu = 0;
            for (int i = 0; i < wielkość_populacji; i++)
            {
                funkcja_celu = populacja[i].Count;
                suma = suma + funkcja_celu;
            }

            List<int> osobniki_dostosowanie = new List<int>();
            int dostosowanie = 0;
            int ruletka = 0;
            for (int i = 0; i < wielkość_populacji; i++)
            {
                dostosowanie = suma - funkcja_celu;
                osobniki_dostosowanie.Add(ruletka + dostosowanie);
                ruletka = ruletka + dostosowanie;
            }
            for (int i = 0; i < osobniki_dostosowanie.Count; i++)
            {
                int rodzic1 = 0;
                int rodzic2 = 0;
                Para = new List<int>();

                while (rodzic1 == rodzic2)
                {
                    int los1 = r.Next(ruletka);
                    int los2 = r.Next(ruletka);
                    for (int j = 0; j < osobniki_dostosowanie.Count; j++)
                    {
                        if (los1 > osobniki_dostosowanie[j])
                        {
                            rodzic1 = j + 1;
                            break;
                        }
                    }
                    if (los1 < osobniki_dostosowanie[0])
                    {
                        rodzic1 = 0;
                    }
                    for (int j = 0; j < osobniki_dostosowanie.Count; j++)
                    {
                        if (los2 <= osobniki_dostosowanie[j])
                        {
                            rodzic2 = j;
                            break;
                        }
                    }
                    if (los2 < osobniki_dostosowanie[0])
                    {
                        rodzic2 = 0;
                    }
                }
                Para.Add(rodzic1);
                Para.Add(rodzic2);
                rodzice.Add(Para);

            }
        }
        #endregion

        #region Krzyżowanie
        private void Krzyżowanie(int wielkość_populacji)
        {
            kolejne_pokolenie.Clear();
            List<int> potomek = new List<int>();

            int prawdopodobieństwo_krzyżowania = (Convert.ToInt32(PrawdopodobieństwoKrzyżowania.Text) * rodzice.Count) / 100;
            int osobniki_większy_zakres = prawdopodobieństwo_krzyżowania * 10 / 100;

            List<int> indeksy_rodziców = new List<int>();
            while (indeksy_rodziców.Count() < osobniki_większy_zakres)
            {
                int indeks = r.Next(osobniki_większy_zakres);
                if (!indeksy_rodziców.Contains(indeks))
                {
                    indeksy_rodziców.Add(indeks);
                }
            }

            for (int i = 0; i < rodzice.Count; i++)
            {
                var ListaSek = ListaNumerowSekwencji();
                var Słownik = NukleotydyDoWykorzystania();

                int rodzic1 = rodzice[i][0];
                int rodzic2 = rodzice[i][1];
                int miejsce_krzyżowania_40_60 = r.Next((40 * (populacja[rodzic1].Count()) / 100), (60 * (populacja[rodzic1].Count()) / 100));
                int miejsce_krzyżowania_0_100 = r.Next(1, populacja[rodzic1].Count() - 1);
                
                
                if (i < prawdopodobieństwo_krzyżowania)
                {
                    if (indeksy_rodziców.Contains(i))
                    {
                        for (int p = 0; p < miejsce_krzyżowania_0_100; p++)
                        {
                            potomek.Add(populacja[rodzic1][p]);
                            int x = populacja[rodzic1][p];
                            char nukleotyd_sek_osobnik100 = lista[x][(n - Słownik[x])];

                            for (int c = 0; c < lista.Count; c++)
                            {
                                if (Słownik[c] == 0)
                                {
                                    continue;
                                }
                                char nukleotyd_lista2 = lista[c][n - Słownik[c]];
                                if (nukleotyd_lista2 == nukleotyd_sek_osobnik100)
                                {
                                    Słownik[c]--;
                                    if (Słownik[c] == 0)
                                    {
                                        ListaSek.Remove(c);
                                    }
                                }
                            }
                        }
                        for (int s = 0; s < populacja[rodzic2].Count; s++)
                        {

                            int z = populacja[rodzic2][s];
                            if (ListaSek.Contains(z))
                            {
                                potomek.Add(z);
                            }
                            else
                            {
                                continue;
                            }
                            char nukleotyd_sek_osobnik2_100 = lista[z][n - Słownik[z]];
                            for (int c = 0; c < lista.Count; c++)
                            {
                                if (Słownik[c] == 0)
                                {
                                    continue;
                                }
                                char nukleotyd_lista2 = lista[c][n - Słownik[c]];

                                if (nukleotyd_lista2 == nukleotyd_sek_osobnik2_100)
                                {
                                    Słownik[c]--;
                                    if (Słownik[c] == 0)
                                    {
                                        ListaSek.Remove(c);
                                    }
                                }
                            }
                        }
                        while (ListaSek.Count != 0)
                        {
                            int WylosowanaSekwencja100 = ListaSek[r.Next(ListaSek.Count)];
                            char WylosowanyNukleotyd100 = lista[WylosowanaSekwencja100][n - Słownik[WylosowanaSekwencja100]];
                            potomek.Add(WylosowanaSekwencja100);
                            for (int k = 0; k < m; k++)
                            {
                                if (Słownik[k] == 0)
                                {
                                    continue;
                                }
                                if (lista[k][n - Słownik[k]] == WylosowanyNukleotyd100)
                                {
                                    Słownik[k]--;
                                    if (Słownik[k] == 0)
                                    {
                                        ListaSek.Remove(k);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int l = 0; l < miejsce_krzyżowania_40_60; l++)
                        {
                            potomek.Add(populacja[rodzic1][l]);
                            int q = populacja[rodzic1][l];
                            char nukleotyd_sek_osobnik = lista[q][(n - Słownik[q])];

                            for (int c = 0; c < lista.Count; c++)
                            {
                                if (Słownik[c] == 0)
                                {
                                    continue;
                                }
                                char nukleotyd_lista = lista[c][n - Słownik[c]];
                                if (nukleotyd_lista == nukleotyd_sek_osobnik)
                                {
                                    Słownik[c]--;
                                    if (Słownik[c] == 0)
                                    {
                                        ListaSek.Remove(c);
                                    }
                                }
                            }
                        }

                        for (int s = 0; s < populacja[rodzic2].Count; s++)
                        {
                            int w = populacja[rodzic2][s];
                            if (ListaSek.Contains(w))
                            {
                                potomek.Add(w);
                            }
                            else
                            {
                                continue;
                            }
                            char nukleotyd_sek_osobnik2 = lista[w][n - Słownik[w]];
                            for (int c = 0; c < lista.Count; c++)
                            {
                                if (Słownik[c] == 0)
                                {
                                    continue;
                                }
                                char nukleotyd_lista2 = lista[c][n - Słownik[c]];

                                if (nukleotyd_lista2 == nukleotyd_sek_osobnik2)
                                {
                                    Słownik[c]--;
                                    if (Słownik[c] == 0)
                                    {
                                        ListaSek.Remove(c);
                                    }
                                }
                            }
                        }
                        while (ListaSek.Count != 0)
                        {
                            int WylosowanaSekwencja = ListaSek[r.Next(ListaSek.Count)];
                            char WylosowanyNukleotyd = lista[WylosowanaSekwencja][n - Słownik[WylosowanaSekwencja]];
                            potomek.Add(WylosowanaSekwencja);
                            for (int k = 0; k < m; k++)
                            {
                                if (Słownik[k] == 0)
                                {
                                    continue;
                                }
                                if (lista[k][n - Słownik[k]] == WylosowanyNukleotyd)
                                {
                                    Słownik[k]--;
                                    if (Słownik[k] == 0)
                                    {
                                        ListaSek.Remove(k);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    int losowanie = r.Next(1);
                    if (losowanie == 0)
                    {
                        potomek = new List<int>(populacja[rodzic1]);
                    }
                    else
                    {
                        potomek = new List<int>(populacja[rodzic2]);
                    }
                }
                kolejne_pokolenie.Add(new List<int>(potomek));

                potomek.Clear();
            }
            for (int i = 0; i < wielkość_populacji; i++)
            {
                populacja[i] = new List<int>(kolejne_pokolenie[i]);
            }
        }
        #endregion

        #region Wprowadzenie mutacji 

        private void Wprowadzenie_mutacji()
        {
            Random r = new Random();
            int prawdopodobieństwo_mutacji = (Convert.ToInt32(PrawdopodobieństwoMutacji.Text) * kolejne_pokolenie.Count) / 100;


            for (int i = 0; i < prawdopodobieństwo_mutacji; i++)
            {
                var ListaSek = ListaNumerowSekwencji();
                var Słownik = NukleotydyDoWykorzystania();

                int wylosowany_osobnik_do_mutacji = r.Next(kolejne_pokolenie.Count);
                int wylosowane_miejsce_mutacji = r.Next(kolejne_pokolenie[wylosowany_osobnik_do_mutacji].Count);
                List<int> skopiowany_osobnik = new List<int>(kolejne_pokolenie[wylosowany_osobnik_do_mutacji]);
                kolejne_pokolenie[wylosowany_osobnik_do_mutacji].Clear();

                for (int j = 0; j < wylosowane_miejsce_mutacji; j++)
                {
                    int y = skopiowany_osobnik[j];
                    if (Słownik[y] == 0)
                    {
                        continue;
                    }
                    kolejne_pokolenie[wylosowany_osobnik_do_mutacji].Add(skopiowany_osobnik[j]);

                    char nukleotyd_sek_osobnik_mutacja = lista[y][n - Słownik[y]];
                    for (int c = 0; c < lista.Count; c++)
                    {
                        if (Słownik[c] == 0)
                        {
                            continue;
                        }

                        char nukleotyd_lista = lista[c][n - Słownik[c]];

                        if (nukleotyd_lista == nukleotyd_sek_osobnik_mutacja)
                        {
                            Słownik[c]--;
                            if (Słownik[c] == 0)
                            {
                                ListaSek.Remove(c);
                            }
                        }
                    }
                }

                if (ListaSek.Count != 0)
                {
                    int WylosowanaSekwencja = ListaSek[r.Next(ListaSek.Count)];

                    if (ListaSek.Count > 1)
                    {
                        while (WylosowanaSekwencja == skopiowany_osobnik[wylosowane_miejsce_mutacji])
                        {
                            WylosowanaSekwencja = ListaSek[r.Next(ListaSek.Count)];
                        }
                    }
                    kolejne_pokolenie[wylosowany_osobnik_do_mutacji].Add(WylosowanaSekwencja);
                    int x = WylosowanaSekwencja;
                    char nukleotyd_sek_osobnik = lista[x][n - Słownik[x]];

                    for (int c = 0; c < lista.Count; c++)
                    {
                        if (Słownik[c] == 0)
                        {
                            continue;
                        }

                        char nukleotyd_lista = lista[c][n - Słownik[c]];

                        if (nukleotyd_lista == nukleotyd_sek_osobnik)
                        {
                            Słownik[c]--;
                            if (Słownik[c] == 0)
                            {
                                ListaSek.Remove(c);
                            }
                        }
                    }
                }
                for (int k = wylosowane_miejsce_mutacji + 1; k < skopiowany_osobnik.Count; k++)
                {
                    int z = skopiowany_osobnik[k];
                    if (ListaSek.Contains(z))
                    {
                        kolejne_pokolenie[wylosowany_osobnik_do_mutacji].Add(skopiowany_osobnik[k]);
                    }
                    else
                    {
                        continue;
                    }
                    char nukleotyd_sek_osobnik2 = lista[z][n - Słownik[z]];

                    for (int c = 0; c < lista.Count; c++)
                    {
                        if (Słownik[c] == 0)
                        {
                            continue;
                        }
                        char nukleotyd_lista2 = lista[c][n - Słownik[c]];

                        if (nukleotyd_lista2 == nukleotyd_sek_osobnik2)
                        {
                            Słownik[c]--;
                            if (Słownik[c] == 0)
                            {
                                ListaSek.Remove(c);
                            }
                        }
                    }
                }
                while (ListaSek.Count != 0)
                {
                    int WylosowanaSekwencja2 = ListaSek[r.Next(ListaSek.Count)];
                    char WylosowanyNukleotyd = lista[WylosowanaSekwencja2][(n - Słownik[WylosowanaSekwencja2])];
                    kolejne_pokolenie[wylosowany_osobnik_do_mutacji].Add(WylosowanaSekwencja2);
                    for (int k = 0; k < m; k++)
                    {
                        if (Słownik[k] == 0)
                        {
                            continue;
                        }
                        if (lista[k][n - Słownik[k]] == WylosowanyNukleotyd)
                        {
                            Słownik[k]--;
                            if (Słownik[k] == 0)
                            {
                                ListaSek.Remove(k);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Funkcja_celu_i_dopasowanie

        public void Funkcja_celu_i_dopasowanie()
        {

            string ścieżka3 = Directory.GetCurrentDirectory() + "\\najlepsze dopasowanie"
                                + m.ToString() + "x" + n.ToString() + ".txt";
            fsa = new FileStream(ścieżka3, FileMode.Append, FileAccess.Write);
            StreamWriter swa = new StreamWriter(fsa);

            var Słownik = NukleotydyDoWykorzystania();
            najlepsze_dopasowanie.Clear();

            for (int j = 0; j < lista.Count; j++)
            {
                najlepsze_dopasowanie.Add(new List<char>());
            }

            for (int i = 0; i < najlepszy_osobnik.Count; i++)
            {
                int x = najlepszy_osobnik[i];
                char nukleotyd_sek_osobnik = lista[x][n - Słownik[x]];

                for (int c = 0; c < lista.Count; c++)
                {
                    if (Słownik[c] == 0)
                    {
                        continue;
                    }
                    char nukleotyd_lista = lista[c][n - Słownik[c]];

                    if (nukleotyd_lista == nukleotyd_sek_osobnik)
                    {
                        Słownik[c]--;
                        najlepsze_dopasowanie[c].Add(nukleotyd_lista);
                    }
                    else
                    {
                        najlepsze_dopasowanie[c].Add('_');
                    }
                }
            }
            string dopasowanie_do_wyświetlenia = "";

            for (int i = 0; i < najlepsze_dopasowanie.Count; i++)
            {
                string linia = "";
                for (int j = 0; j < najlepsze_dopasowanie[i].Count; j++)
                {
                    linia += najlepsze_dopasowanie[i][j];

                }
                dopasowanie_do_wyświetlenia += linia + "\r\n";
            }
            podgląd_dopasowania.Clear();
            podgląd_dopasowania.Text = dopasowanie_do_wyświetlenia.ToUpper();
            podgląd_najlepszej_wartości_funkcji.Text = (najlepszy_osobnik.Count).ToString();

            swa.WriteLine("Wartość funkcji celu: " + najlepszy_osobnik.Count.ToString());
            swa.WriteLine("Czas działania programu: " + Convert.ToString(sekundy));
            swa.Write(dopasowanie_do_wyświetlenia);
            swa.Close();
            fsa.Close();
        }

        #endregion


        #region Metaheurystyka Start
        private void MetaheurystykaStart()
        {

            populacja.Clear();
            najlepszy_osobnik.Clear();

            int Counter = 0;
            int liczba_iteracji;
            bool czas;
            int liczba_zadanych_iteracji;
            double zadany_czas;
            DateTime start_czas = DateTime.Now;
            DateTime stop_czas;
            int m = lista.Count - 1;
            int n = lista[0].Length - 1;
            bool czy_petla_pierwsza = true;
            int iteracje_bez_poprawy;
            int dotychczasowe_iteracje;
            int wielkość_populacji = Convert.ToInt32(WielkośćPopulacji.Text);


            if (if_stop_clicked() == true)
            {
                return;
            }

            // tworzenie początkowej populacji 
            TworzeniePopulacji(wielkość_populacji, m, n);

            if (if_stop_clicked() == true)
            {
                return;
            }

            if (iteracje.Checked)
            {
                iteracje_bez_poprawy = 1;
                dotychczasowe_iteracje = 0;
                czas = true;
                zadany_czas = 0;
                liczba_zadanych_iteracji = Convert.ToInt32(ile_iteracji.Text);
                liczba_iteracji = 0;
            }
            else if (czasy.Checked)
            {
                iteracje_bez_poprawy = 1;
                dotychczasowe_iteracje = 0;
                liczba_iteracji = 0;
                liczba_zadanych_iteracji = 1;
                czas = true;
                zadany_czas = Convert.ToDouble(jaki_czas.Text);
            }
            else
            {
                iteracje_bez_poprawy = 0;
                dotychczasowe_iteracje = 0;
                liczba_iteracji = 0;
                liczba_zadanych_iteracji = 1;
                czas = true;
                zadany_czas = 0;
            }

            while (czas && liczba_iteracji < liczba_zadanych_iteracji)
            {
                //wybór rodziców
                WybórRodziców(wielkość_populacji);
                if (if_stop_clicked() == true)
                {
                    return;
                }

                // krzyżowanie
                Krzyżowanie(wielkość_populacji);
                if (if_stop_clicked() == true)
                {
                    return;
                }

                //wprowadzenie mutacji
                Wprowadzenie_mutacji();
                if (if_stop_clicked() == true)
                {
                    return;
                }

                if (czy_petla_pierwsza)
                {
                    int najkrótsze_dopasowanie = kolejne_pokolenie[0].Count;
                    int indeks_najlepszego_osobnika = 0;

                    for (int i = 0; i < kolejne_pokolenie.Count; i++)
                    {
                        if (kolejne_pokolenie[i].Count < najkrótsze_dopasowanie)
                        {
                            najkrótsze_dopasowanie = kolejne_pokolenie[i].Count;
                            indeks_najlepszego_osobnika = i;
                        }

                    }
                    najlepsza_funkcja_celu = najkrótsze_dopasowanie;
                    najlepszy_osobnik = new List<int>(kolejne_pokolenie[indeks_najlepszego_osobnika]);
                    czy_petla_pierwsza = false;
                }
                else
                {
                    int najkrótsze_dopasowanie = kolejne_pokolenie[0].Count;
                    int indeks_najlepszego_osobnika = 0;

                    for (int i = 0; i < kolejne_pokolenie.Count; i++)
                    {
                        if (kolejne_pokolenie[i].Count < najkrótsze_dopasowanie)
                        {
                            najkrótsze_dopasowanie = kolejne_pokolenie[i].Count;
                            indeks_najlepszego_osobnika = i;
                        }
                    }
                    if (najkrótsze_dopasowanie < najlepsza_funkcja_celu)
                    {
                        najlepsza_funkcja_celu = najkrótsze_dopasowanie;
                        najlepszy_osobnik = new List<int>(kolejne_pokolenie[indeks_najlepszego_osobnika]);
                    }
                    if (najkrótsze_dopasowanie >= najlepsza_funkcja_celu)
                    {
                        Counter++;
                    }
                }

                if (iteracje.Checked)
                {
                    liczba_iteracji++;
                    if (liczba_iteracji > liczba_zadanych_iteracji)
                    {
                        czas = false;
                        t.Stop();
                        Stop = true;
                        Active = false;
                    }
                }
                if (czasy.Checked)
                {
                    DateTime end = DateTime.Now;
                    double ts = (end - start_czas).TotalSeconds;
                    if (ts > zadany_czas)
                    {
                        czas = false;
                        t.Stop();
                        Stop = true;
                        Active = false;
                    }
                }
                if (iter_bez_popraw.Checked)
                {
                    while (Counter >= Convert.ToInt32(bez_poprawy.Text))
                    {
                        czas = false;
                        t.Stop();
                        Stop = true;
                        Active = false;
                        break;

                    }
                }
            }
            stop_czas = DateTime.Now;
            sekundy = (stop_czas - start_czas).TotalSeconds;
            t.Stop();
            Stop = true;
            Active = false;
        }
        #endregion



    }
}
