# 🕵️ casusmusunnesin?

Modern web tabanlı "Casus" oyunu - arkadaşlarınızla birlikte oynayabileceğiniz interaktif bir sosyal dedüksiyon oyunu.

## 🎮 Oyun Hakkında

"Casus" oyunu, oyuncuların arasından birinin gizli bir kelimeyi bilen casus olduğu, diğer oyuncuların ise bu kelimeyi tahmin etmeye çalıştığı bir sosyal dedüksiyon oyunudur.

### 🎯 Oyun Kuralları
- Oyuncular arasından rastgele bir casus seçilir
- Casus, diğer oyuncuların bilmediği gizli kelimeyi bilir
- Diğer oyuncular ortak bir kelimeyi bilir
- Oyun süresi boyunca oyuncular birbirlerine sorular sorar
- Süre sonunda oyuncular casus olduğunu düşündükleri kişiyi oylar
- Eğer casus oylanırsa oyuncular kazanır, aksi takdirde casus kazanır

## 🚀 Özellikler

### ✅ Mevcut Özellikler
- **Gerçek Zamanlı Oyun**: SignalR ile anlık iletişim
- **Oda Sistemi**: Özel oda kodları ile oyun kurma
- **Kullanıcı Sistemi**: Kayıt olma ve giriş yapma
- **Premium Özellikler**: Abonelik sistemi
- **İstatistikler**: Oyun performansınızı takip edin
- **Responsive Tasarım**: Mobil ve masaüstü uyumlu

### 🔮 Gelecek Özellikler
- **Turnuva Sistemi**: Ödüllü turnuvalar
- **Özel Kategoriler**: Premium kullanıcılar için özel kelimeler
- **API Erişimi**: Geliştiriciler için API
- **Mobil Uygulama**: iOS ve Android uygulamaları

## 🛠️ Teknolojiler

### Backend
- **ASP.NET Core 8.0**: Modern web framework
- **Entity Framework Core**: ORM ve veritabanı yönetimi
- **SignalR**: Gerçek zamanlı iletişim
- **SQL Server**: Veritabanı
- **Redis**: Önbellekleme (production)

### Frontend
- **Bootstrap 5**: Responsive UI framework
- **jQuery**: JavaScript kütüphanesi
- **Font Awesome**: İkonlar
- **Chart.js**: İstatistik grafikleri

### DevOps & Deployment
- **Docker**: Containerization
- **Azure**: Cloud hosting
- **GitHub Actions**: CI/CD
- **Azure DevOps**: Pipeline management

## 📦 Kurulum

### Gereksinimler
- .NET 8.0 SDK
- SQL Server (LocalDB veya SQL Server Express)
- Visual Studio 2022 veya VS Code

### Yerel Geliştirme

1. **Repository'yi klonlayın**
```bash
git clone https://github.com/your-username/casus-oyunu.git
cd casus-oyunu
```

2. **Bağımlılıkları yükleyin**
```bash
dotnet restore
```

3. **Veritabanını oluşturun**
```bash
dotnet ef database update
```

4. **Uygulamayı çalıştırın**
```bash
dotnet run
```

5. **Tarayıcıda açın**
```
https://localhost:7001
```

### Docker ile Çalıştırma

1. **Docker Compose ile başlatın**
```bash
docker-compose up -d
```

2. **Tarayıcıda açın**
```
http://localhost:8080
```

## 🚀 Production Deployment

### Azure Deployment

1. **Azure Web App oluşturun**
2. **SQL Database oluşturun**
3. **Connection string'i güncelleyin**
4. **GitHub Actions ile deploy edin**

### Environment Variables

Production ortamında aşağıdaki environment variable'ları ayarlayın:

```bash
ConnectionStrings__DefaultConnection=your-connection-string
Azure__SignalR__ConnectionString=your-signalr-connection
Payment__Stripe__SecretKey=your-stripe-secret
Email__Password=your-email-password
```

## 💰 Monetizasyon

### Premium Özellikler
- **Temel (₺19.99/ay)**: Reklamsız oyun, özel kategoriler, istatistikler
- **Premium (₺39.99/ay)**: Turnuva katılımı, özel temalar, öncelikli destek
- **Pro (₺79.99/ay)**: Turnuva oluşturma, API erişimi, özel etkinlikler

### Gelir Kaynakları
- Premium abonelikler
- Turnuva giriş ücretleri
- Reklam gelirleri (ücretsiz kullanıcılar)
- API kullanım ücretleri

## 🔧 Geliştirme

### Proje Yapısı
```
casus-oyunu/
├── Controllers/          # MVC Controllers
├── Models/              # Entity models
├── Views/               # Razor views
├── Data/                # DbContext ve migrations
├── Services/            # Business logic services
├── Hubs/               # SignalR hubs
└── wwwroot/            # Static files
```

### Yeni Özellik Ekleme

1. **Model oluşturun** (`Models/` klasöründe)
2. **Migration ekleyin** (`dotnet ef migrations add FeatureName`)
3. **Controller oluşturun** (`Controllers/` klasöründe)
4. **View ekleyin** (`Views/` klasöründe)
5. **Test edin** ve commit edin

## 🧪 Test

```bash
# Unit testleri çalıştır
dotnet test

# Integration testleri çalıştır
dotnet test --filter Category=Integration
```

## 📊 Performans

### Optimizasyonlar
- **Caching**: Memory cache ve Redis
- **Database Indexing**: Performans için optimize edilmiş indeksler
- **SignalR**: Gerçek zamanlı iletişim
- **CDN**: Static dosyalar için CDN kullanımı

### Monitoring
- **Application Insights**: Azure monitoring
- **Logging**: Structured logging
- **Health Checks**: Uygulama sağlık kontrolü

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Commit edin (`git commit -m 'Add some AmazingFeature'`)
4. Push edin (`git push origin feature/AmazingFeature`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakın.

## 📞 İletişim

- **Website**: https://casusmusunnesin.com
- **Email**: info@casusmusunnesin.com
- **GitHub**: https://github.com/your-username/casus-oyunu

## 🙏 Teşekkürler

Bu projeyi mümkün kılan tüm açık kaynak kütüphanelere ve topluluğa teşekkürler!

---

**🕵️ casusmusunnesin?** - Gerçek casus gibi oynayın! 🎮 