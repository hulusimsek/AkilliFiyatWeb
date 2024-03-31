using System;
using System.Collections.Generic;
using System.Linq;

namespace AkilliFiyatWeb.Services
{
    public class KelimeKontrol
    {
        private List<string> istenmeyenKelimeler = new List<string>(); // İstenmeyen kelimeler listesi

        public string Temizle(string cumle)
        {
            cumle = cumle.Trim();
            string[] kelimeler = cumle.Split(new char[] { ' ', ',', '.', '!', '?', '*' }, StringSplitOptions.RemoveEmptyEntries);

            List<string> temizlenmisKelimeler = new List<string>();

            foreach (string kelime in kelimeler)
            {
                if (!istenmeyenKelimeler.Contains(kelime.ToUpper()) && !ContainsNumbers(kelime))
                {
                    temizlenmisKelimeler.Add(kelime);
                }
            }

            return string.Join(" ", temizlenmisKelimeler);
        }

        private bool ContainsNumbers(string kelime)
        {
            return kelime.Any(char.IsDigit);
        }

        public double BenzerlikHesapla(string s1, string s2)
        {
            HashSet<char> set1 = new HashSet<char>(s1);
            HashSet<char> set2 = new HashSet<char>(s2);

            int intersectionSize = set1.Intersect(set2).Count();
            int unionSize = set1.Count + set2.Count - intersectionSize;

            return (double)intersectionSize / unionSize;
        }

        public double BenzerlikHesapla2(string s1, string s2)
        {
            int[,] distanceMatrix = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
            {
                distanceMatrix[i, 0] = i;
            }
            for (int j = 1; j <= s2.Length; j++)
            {
                distanceMatrix[0, j] = j;
            }

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int substitutionCost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    distanceMatrix[i, j] = Math.Min(Math.Min(
                                    distanceMatrix[i - 1, j] + 1,
                                    distanceMatrix[i, j - 1] + 1),
                            distanceMatrix[i - 1, j - 1] + substitutionCost);
                }
            }

            int maxLen = Math.Max(s1.Length, s2.Length);
            double similarity = (double)(maxLen - distanceMatrix[s1.Length, s2.Length]) / maxLen;

            return similarity;
        }

        public string ConvertTurkishToEnglish(string metin)
        {
            metin = metin.ToUpper();
            string[] turkceKarakterler = { "ç", "ğ", "ı", "ö", "ş", "ü", "Ç", "Ğ", "I", "İ", "Ö", "Ş", "Ü" };
            string[] ingilizceKarakterler = { "c", "g", "i", "o", "s", "u", "C", "G", "I", "I", "O", "S", "U" };

            for (int i = 0; i < turkceKarakterler.Length; i++)
            {
                metin = metin.Replace(turkceKarakterler[i], ingilizceKarakterler[i]);
            }

            return metin;
        }

        public string ConvertTurkishToEnglish2(string metin)
        {
            string[] turkceKarakterler = { "ç", "ğ", "ı", "ö", "ş", "ü", "Ç", "Ğ", "I", "İ", "Ö", "Ş", "Ü" };
            string[] ingilizceKarakterler = { "c", "g", "i", "o", "s", "u", "C", "G", "I", "I", "O", "S", "U" };

            for (int i = 0; i < turkceKarakterler.Length; i++)
            {
                metin = metin.Replace(turkceKarakterler[i], ingilizceKarakterler[i]);
            }

            return metin;
        }

        public bool IkinciKelime(string arananString, string digerString)
        {
            arananString = ConvertTurkishToEnglish(arananString);
            digerString = ConvertTurkishToEnglish(digerString);
            string[] arananKelimeler = arananString.Split(' ');

            if (arananKelimeler.Length >= 2)
            {
                return digerString.Contains(arananKelimeler[1]);
            }
            else
            {
                return digerString.Contains(arananKelimeler[0]);
            }
        }

        public int IkinciKelime2(string arananString, string digerString)
        {
            arananString = ConvertTurkishToEnglish(arananString);
            digerString = ConvertTurkishToEnglish(digerString);
            string[] arananKelimeler = arananString.Split(new char[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
            string[] digerStringler = digerString.Split(new char[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);

            List<string> digerKelimelerList = new List<string>(digerStringler);
            int katsayi = 0;
            if (arananKelimeler.Length >= 3)
            {
                if (digerString.Contains(arananKelimeler[0]))
                {
                    if (digerKelimelerList.Contains(arananKelimeler[0]))
                    {
                        int a = 0;
                        foreach (string kelime in arananKelimeler)
                        {
                            if (digerString.Contains(kelime))
                            {
                                a++;
                            }
                        }
                        if (a == arananKelimeler.Length)
                        {
                            katsayi += 40;
                        }
                        katsayi += 20;
                    }
                    else
                    {
                        katsayi += 10;
                    }
                }
                else
                {
                    katsayi += 0;
                }
            }
            if (arananKelimeler.Length >= 2)
            {
                if (arananString.Any(char.IsDigit))
                {
                    if (arananKelimeler.Length >= 3 && digerKelimelerList.Contains(arananKelimeler[2]))
                    {
                        int a = 0;
                        foreach (string kelime in arananKelimeler)
                        {
                            if (digerString.Contains(kelime))
                            {
                                a++;
                            }
                        }
                        if (a == arananKelimeler.Length)
                        {
                            katsayi += 40;
                        }
                        katsayi += 20;
                    }
                    else if (arananKelimeler.Length >= 2 && digerKelimelerList.Contains(arananKelimeler[1]))
                    {
                        int a = 0;
                        foreach (string kelime in arananKelimeler)
                        {
                            if (digerString.Contains(kelime))
                            {
                                a++;
                            }
                        }
                        if (a == arananKelimeler.Length)
                        {
                            katsayi += 40;
                        }
                        katsayi += 20;
                    }
                    else
                    {
                        katsayi += 0;
                    }
                }
                else if (arananString.Any(char.IsDigit))
                {
                    if (arananKelimeler.Length >= 4)
                    {
                        if (digerKelimelerList.Contains(arananKelimeler[1]))
                        {
                            int a = 0;
                            foreach (string kelime in arananKelimeler)
                            {
                                if (digerString.Contains(kelime))
                                {
                                    a++;
                                }
                            }
                            if (a == arananKelimeler.Length)
                            {
                                katsayi += 40;
                            }
                            katsayi += 20;
                        }
                    }
                    else if (arananKelimeler.Length >= 2)
                    {
                        if (digerKelimelerList.Contains(arananKelimeler[0]))
                        {
                            int a = 0;
                            foreach (string kelime in arananKelimeler)
                            {
                                if (digerString.Contains(kelime))
                                {
                                    a++;
                                }
                            }
                            if (a == arananKelimeler.Length)
                            {
                                katsayi += 40;
                            }
                            katsayi += 20;
                        }
                    }
                    else
                    {
                        katsayi += 0;
                    }
                }
                if (digerString.Contains(arananKelimeler[1]))
                {
                    if (digerKelimelerList.Contains(arananKelimeler[1]))
                    {
                        int a = 0;
                        foreach (string kelime in arananKelimeler)
                        {
                            if (digerString.Contains(kelime))
                            {
                                a++;
                            }
                        }
                        if (a == arananKelimeler.Length)
                        {
                            katsayi += 40;
                        }
                        katsayi += 20;
                    }
                    else
                    {
                        katsayi += 10;
                    }
                }
                else
                {
                    katsayi += 0;
                }
            }
            else
            {
                if (digerString.Contains(arananKelimeler[0]))
                {
                    if (digerKelimelerList.Contains(arananKelimeler[0]))
                    {
                        int a = 0;
                        foreach (string kelime in arananKelimeler)
                        {
                            if (digerString.Contains(kelime))
                            {
                                a++;
                            }
                        }
                        if (a == arananKelimeler.Length)
                        {
                            katsayi += 40;
                        }
                        katsayi += 20;
                    }
                    else
                    {
                        katsayi += 10;
                    }
                }
                else
                {
                    katsayi += 0;
                }
            }

            return katsayi;
        }
    }
}
