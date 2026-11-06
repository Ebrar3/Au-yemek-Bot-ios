import 'package:flutter/material.dart';
import '../models/menu.dart';
import '../services/api_service.dart';
import 'food_detail_rating_screen.dart';

/// Aylık Yemek Takvimi Ekranı (3. Hafta Görevi).
/// Seçilen ayın tüm öğle veya akşam menülerini Web API'den çeker,
/// takvim ızgarasında resmi tatilleri ve menü günlerini işaretler.
class MonthlyCalendarScreen extends StatefulWidget {
  const MonthlyCalendarScreen({super.key});

  @override
  State<MonthlyCalendarScreen> createState() => _MonthlyCalendarScreenState();
}

class _MonthlyCalendarScreenState extends State<MonthlyCalendarScreen> {
  final ApiService _api = ApiService();
  int _yil = 2026;
  int _ay = 10; // Örnek Excel menülerinin bulunduğu Ekim 2026 ile başlar
  bool _aksamMi = false;
  int _seciliGun = 1;

  late Future<List<Menu>> _aylikMenuFuture;

  static const _ayAdlari = [
    'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
    'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'
  ];

  static const _gunBasliklari = ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz'];

  @override
  void initState() {
    super.initState();
    _verileriYukle();
  }

  void _verileriYukle() {
    _aylikMenuFuture = _api.getAylikMenuler(
      yil: _yil,
      ay: _ay,
      ogun: _aksamMi ? 2 : 1,
    );
  }

  void _ayDegistir(int fark) {
    setState(() {
      final yeniTarih = DateTime(_yil, _ay + fark, 1);
      _yil = yeniTarih.year;
      _ay = yeniTarih.month;
      _seciliGun = 1;
      _verileriYukle();
    });
  }

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        // 1) Ay Seçici Üst Kart
        Card(
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                IconButton(
                  tooltip: 'Önceki Ay',
                  onPressed: () => _ayDegistir(-1),
                  icon: const Icon(Icons.chevron_left),
                ),
                Text(
                  '${_ayAdlari[_ay - 1]} $_yil Yemek Takvimi',
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
                IconButton(
                  tooltip: 'Sonraki Ay',
                  onPressed: () => _ayDegistir(1),
                  icon: const Icon(Icons.chevron_right),
                ),
              ],
            ),
          ),
        ),
        const SizedBox(height: 12),

        // 2) Öğle / Akşam Seçimi
        SegmentedButton<bool>(
          segments: const [
            ButtonSegment(value: false, label: Text('☀️ Öğle Takvimi')),
            ButtonSegment(value: true, label: Text('🌙 Akşam Takvimi')),
          ],
          selected: {_aksamMi},
          onSelectionChanged: (secim) => setState(() {
            _aksamMi = secim.first;
            _verileriYukle();
          }),
        ),
        const SizedBox(height: 16),

        // 3) Takvim Izgarası ve Seçili Günün Menü Detayı
        FutureBuilder<List<Menu>>(
          future: _aylikMenuFuture,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Padding(
                padding: EdgeInsets.all(32),
                child: Center(child: CircularProgressIndicator()),
              );
            }

            if (snapshot.hasError) {
              return Card(
                color: Theme.of(context).colorScheme.errorContainer,
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    children: [
                      const Icon(Icons.cloud_off, size: 36),
                      const SizedBox(height: 8),
                      Text(
                        'Aylık takvim verisi alınamadı.\n${snapshot.error}',
                        textAlign: TextAlign.center,
                      ),
                      const SizedBox(height: 8),
                      FilledButton.icon(
                        onPressed: () => setState(_verileriYukle),
                        icon: const Icon(Icons.refresh),
                        label: const Text('Tekrar Dene'),
                      ),
                    ],
                  ),
                ),
              );
            }

            final menuler = snapshot.data ?? const [];
            final gunSozlugu = <int, Menu>{
              for (final m in menuler)
                if (m.tarih.year == _yil && m.tarih.month == _ay) m.tarih.day: m,
            };

            final aydakiGunSayisi = DateTime(_yil, _ay + 1, 0).day;
            final ilkGununHaftaIndeksi = DateTime(_yil, _ay, 1).weekday - 1; // 0: Pzt .. 6: Paz
            final seciliMenu = gunSozlugu[_seciliGun];

            return Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                // Haftanın günleri başlığı
                Row(
                  children: _gunBasliklari
                      .map(
                        (g) => Expanded(
                          child: Center(
                            child: Text(
                              g,
                              style: Theme.of(context).textTheme.labelMedium?.copyWith(
                                    fontWeight: FontWeight.bold,
                                  ),
                            ),
                          ),
                        ),
                      )
                      .toList(),
                ),
                const SizedBox(height: 8),

                // Takvim hücreleri
                GridView.builder(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  itemCount: ilkGununHaftaIndeksi + aydakiGunSayisi,
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 7,
                    mainAxisSpacing: 6,
                    crossAxisSpacing: 6,
                  ),
                  itemBuilder: (context, index) {
                    if (index < ilkGununHaftaIndeksi) {
                      return const SizedBox.shrink();
                    }
                    final gun = index - ilkGununHaftaIndeksi + 1;
                    final gunMenusu = gunSozlugu[gun];
                    final seciliMi = gun == _seciliGun;
                    final tatilMi = gunMenusu?.tatilMi ?? false;
                    final menuVarMi = gunMenusu != null && !tatilMi;

                    Color arkaPlan = Colors.transparent;
                    if (seciliMi) {
                      arkaPlan = Theme.of(context).colorScheme.primary;
                    } else if (tatilMi) {
                      arkaPlan = Colors.orange.shade100;
                    } else if (menuVarMi) {
                      arkaPlan = Theme.of(context).colorScheme.secondaryContainer;
                    }

                    return InkWell(
                      borderRadius: BorderRadius.circular(10),
                      onTap: () => setState(() => _seciliGun = gun),
                      child: Container(
                        decoration: BoxDecoration(
                          color: arkaPlan,
                          borderRadius: BorderRadius.circular(10),
                          border: Border.all(
                            color: seciliMi
                                ? Theme.of(context).colorScheme.primary
                                : Colors.grey.shade300,
                          ),
                        ),
                        child: Column(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Text(
                              '$gun',
                              style: TextStyle(
                                fontWeight: FontWeight.bold,
                                color: seciliMi ? Colors.white : Colors.black87,
                              ),
                            ),
                            if (tatilMi)
                              Icon(
                                Icons.celebration,
                                size: 12,
                                color: seciliMi ? Colors.white : Colors.deepOrange,
                              )
                            else if (menuVarMi)
                              Icon(
                                Icons.restaurant,
                                size: 12,
                                color: seciliMi
                                    ? Colors.white
                                    : Theme.of(context).colorScheme.primary,
                              ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
                const SizedBox(height: 16),

                // Seçili günün detay kartı
                _SeciliGunMenuDetayi(
                  tarihMetni: '$_seciliGun ${_ayAdlari[_ay - 1]} $_yil',
                  ogunMetni: _aksamMi ? 'Akşam Menüsü' : 'Öğle Menüsü',
                  menu: seciliMenu,
                ),
              ],
            );
          },
        ),
      ],
    );
  }
}

class _SeciliGunMenuDetayi extends StatelessWidget {
  final String tarihMetni;
  final String ogunMetni;
  final Menu? menu;

  const _SeciliGunMenuDetayi({
    required this.tarihMetni,
    required this.ogunMetni,
    required this.menu,
  });

  @override
  Widget build(BuildContext context) {
    if (menu == null) {
      return Card(
        child: Padding(
          padding: const EdgeInsets.all(20),
          child: Text(
            '$tarihMetni ($ogunMetni) için kayıtlı menü bulunmuyor (Hafta sonu veya menü girilmemiş).',
            textAlign: TextAlign.center,
          ),
        ),
      );
    }

    if (menu!.tatilMi) {
      return Card(
        color: Colors.amber.shade50,
        child: ListTile(
          leading: const Icon(Icons.celebration, color: Colors.deepOrange, size: 36),
          title: Text('$tarihMetni – Resmi Tatil'),
          subtitle: Text(menu!.aciklama ?? 'Yemekhane hizmeti bulunmamaktadır.'),
        ),
      );
    }

    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  '$tarihMetni • $ogunMetni',
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                      ),
                ),
                if (menu!.toplamKalori != null)
                  Chip(label: Text('${menu!.toplamKalori} kcal')),
              ],
            ),
            const Divider(),
            ...menu!.yemekler.map(
              (y) => ListTile(
                contentPadding: EdgeInsets.zero,
                title: Text(y.ad),
                subtitle: Text(
                  y.kalori != null
                      ? '${y.kategoriAdi} • ${y.kalori} kcal'
                      : y.kategoriAdi,
                ),
                trailing: const Icon(Icons.chevron_right),
                onTap: () => Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (_) => FoodDetailRatingScreen(yemek: y),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
