using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WordCounter
{
    class Program
    {
        static char[] delimiterChars = { '.', ':', '?', '!' };// Cümleleri birbirinden ayırmak için cümleyi bitiren noktalama işaretleri tanımlandı.
        static int threadCount = 5;// Default thread sayısı tanımlandı
        static int completedThread = 0;// Tüm threadlerin tamamlandıklarını kontrol etmek için bu parametre kullanıldı
        static List<string> wordCounts;// Cümle içindeki tüm kelimeler bu listeye eklenir.
        static string[] sentence;
        static int listIndex = 0;
        private static readonly object balanceLock = new object();//Aynı anda farklı threadlerin aynı cümleye atanmaması için o fonksiyon içinde lock objesi kullanıldı

        static void Main(string[] args)
        {
            try
            {
                #region Yapılandırma
                Console.WriteLine("Dosya yolu giriniz : (Örnek : D:\\Educations\\test.txt)");
                string fileName = Console.ReadLine();
                Console.WriteLine("Yardımcı thread sayısı girmek ister misiniz? (E/H, varsayılan değer = 5)");
                string threadCons = Console.ReadLine();
                if (threadCons == "E")
                {
                    Console.WriteLine("Yardımcı thread sayısını giriniz : ");
                    threadCount = Convert.ToInt32(Console.ReadLine());
                }
                Console.WriteLine("İşlemler yapılıyor.");
                Console.WriteLine("");
                Console.WriteLine("");

                wordCounts = new List<string>();
                #endregion

                #region Dosya Okuma
                //string text = FileRead("D:\\Educations\\test.txt")
                string text = FileRead(fileName)
                    .Replace("\r", "")
                    .Replace("\n", " ")
                    .Replace("\t", " ")
                    .Replace(",", "")
                    .Replace(";", "")
                    .Replace("“", "")
                    .Replace("”", "")
                    .Replace("...", ".")
                    .Replace("-", "");
                #endregion

                sentence = text.Split(delimiterChars);// Cümleler daha önce tanımlanan noktalama işaretlerine göre ayrılıyor ve diziye atılıyor.

                for (int i = 0; i < threadCount; i++)
                {
                    Thread thread = new Thread(() =>
                    {
                        JobAssignment();
                        completedThread++;
                        //Action action = () => completedThread++;
                    });
                    thread.Start();
                }

                while (threadCount != completedThread)
                {

                } //Tüm threadlerin tamamlanması beklenir

                int avgSkor = wordCounts.Count / (sentence.Length - 1); // Cümle içindeki ortalama kelime sayısı
                int sentenceCount = sentence.Length - 1;// Cümle sayısı

                //Liste içindeki kelimelerin kaç defa tekrarlandığı bulunarak yeni bir liste oluşturulur.6
                var wordCountsSkor =
                        from n in wordCounts
                        group n by n into wordCountGroup
                        select new
                        {
                            Word = wordCountGroup.Key,
                            Count = wordCountGroup.Count(),
                        };


                #region Ekrana yazdırma işlemleri
                Console.WriteLine("Sentence Count  : " + sentenceCount);
                Console.WriteLine("Avg. Word Count : " + avgSkor);
                wordCountsSkor.OrderByDescending(x => x.Count).ToList().ForEach(x => { Console.WriteLine(x.Word + " : " + x.Count); });
                #endregion

                #region Kelimelerin kullanım sayılarına göre sıralanması - Kullanılmıyor
                //var sortedWordCountsSkor = from n in wordCountsSkor
                //                           orderby n.Count descending
                //                           select new
                //                           {
                //                               n.Word,
                //                               n.Count,
                //                           };
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        #region Fonksiyonlar

        public static string FileRead(string fileName)
        {
            //Dosya okuma fonksiyonu
            string text = "";
            using (StreamReader sr = new StreamReader(fileName))
            {
                String line = sr.ReadToEnd();
                text += line;
            }
            return text;
        }

        public static void JobAssignment()
        {
            lock (balanceLock)
            {
                while (listIndex < sentence.Length - 1)
                {
                    //Her cümle sırayla gelen threadelere atanır.
                    //Aynı cümleyi aynı anda gelen farklı threadlere atmamak için lock objesi kullanıldı.
                    WordCounter(listIndex);
                    listIndex++;
                }
            }
        }

        public static void WordCounter(int listIndex)
        {
            string[] words = sentence[listIndex].Trim().Split(' ');//Cümle içindeki boşluklara göre kelimelere ayrılır ve diziye atılır.
            foreach (var item in words)
            {
                //Cümle içinde kullanılan tüm kelimeler listeye eklenir
                wordCounts.Add(item);
            }
        }

        #endregion

    }
}
