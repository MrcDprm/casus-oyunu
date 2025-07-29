using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace casus_oyunu.Models
{
    [NotMapped]
    public class Category
    {
        public string Name { get; set; }
        public List<string> Words { get; set; }
    }

    public static class CategoryData
    {
        public static List<Category> Categories = new List<Category>
        {
            new Category
            {
                Name = "Spor",
                Words = new List<string> { "Stadyum", "Yüzme Havuzu", "Tenis Kortu", "Basketbol Sahası", "Spor Salonu", "Kayak Merkezi", "Golf Sahası", "Maraton Parkuru", "Boks Salonu", "Futbol Sahası", "Voleybol Sahası", "Bisiklet Parkuru", "Okçuluk Alanı", "Beyzbol Sahası", "Kriket Sahası" }
            },
            new Category
            {
                Name = "Tatil",
                Words = new List<string> { "Plaj", "Dağ Evi", "Kamp Alanı", "Tatil Köyü", "Karavan", "Bungalov", "Lüks Otel", "Pansiyon", "Tatil Adası", "Yat", "Dağ Zirvesi", "Orman Kampı", "Termal Otel", "Tatil Kasabası", "Çadır" }
            },
            new Category
            {
                Name = "Eğitim",
                Words = new List<string> { "Okul", "Kütüphane", "Üniversite", "Dershane", "Sınıf", "Laboratuvar", "Konferans Salonu", "Amfi", "Öğrenci Yurdu", "Atölye", "Bilgisayar Odası", "Müzik Sınıfı", "Spor Salonu", "Öğretmenler Odası", "Kantin" }
            },
            new Category
            {
                Name = "Sağlık",
                Words = new List<string> { "Hastane", "Eczane", "Klinik", "Ambulans", "Dişçi", "Ameliyathane", "Acil Servis", "Göz Hastanesi", "Fizik Tedavi Merkezi", "Psikiyatri Kliniği", "Aile Sağlığı Merkezi", "Doğumhane", "Kan Merkezi", "Radyoloji", "Rehabilitasyon Merkezi" }
            },
            new Category
            {
                Name = "Ulaşım",
                Words = new List<string> { "Havalimanı", "Tren Garı", "Otobüs Terminali", "Liman", "Metro İstasyonu", "Taksi Durağı", "Feribot", "Otopark", "Helikopter Pisti", "Tramvay", "Otoban", "Bisiklet Yolu", "Gemi", "Uçak", "Otobüs" }
            },
            new Category
            {
                Name = "Yeme-İçme",
                Words = new List<string> { "Restoran", "Kafe", "Bar", "Pastane", "Fast Food", "Lokanta", "Çay Bahçesi", "Büfe", "Pideci", "Kebapçı", "Balıkçı", "Kahve Dükkanı", "Dondurmacı", "Meze Barı", "Şarap Evi" }
            },
            new Category
            {
                Name = "Sanat",
                Words = new List<string> { "Tiyatro", "Sinema", "Müzikhol", "Sergi Salonu", "Heykel Atölyesi", "Resim Galerisi", "Opera", "Bale Salonu", "Konser Alanı", "Fotoğraf Stüdyosu", "Dans Stüdyosu", "Müzik Stüdyosu", "Sanat Okulu", "Kukla Tiyatrosu", "Grafiti Alanı" }
            },
            new Category
            {
                Name = "Alışveriş",
                Words = new List<string> { "AVM", "Süpermarket", "Butik", "Manav", "Kuyumcu", "Kitapçı", "Elektronik Mağazası", "Ayakkabıcı", "Pazaryeri", "Çiçekçi", "Oyuncakçı", "Eczane", "Kırtasiye", "Giyim Mağazası", "Hediyelikçi" }
            },
            new Category
            {
                Name = "Doğa",
                Words = new List<string> { "Orman", "Göl", "Dağ", "Şelale", "Mağara", "Milli Park", "Botanik Bahçesi", "Çöl", "Yayla", "Vadi", "Sazlık", "Deniz", "Kanyon", "Adacık", "Fırtına Vadisi" }
            },
            new Category
            {
                Name = "Eğlence",
                Words = new List<string> { "Lunapark", "Oyun Salonu", "Bowling Salonu", "Bilardo Salonu", "Karaoke Bar", "Disko", "Paintball Alanı", "Go-Kart Pisti", "Kaçış Odası", "Sanal Gerçeklik Merkezi", "Trambolin Parkı", "Su Parkı", "Atlıkarınca", "Çocuk Parkı", "Mini Golf" }
            }
        };
    }
} 