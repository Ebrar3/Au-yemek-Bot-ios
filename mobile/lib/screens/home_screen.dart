import 'package:flutter/material.dart';
import '../models/menu.dart';
import '../models/yemek.dart';
import '../services/api_service.dart';
import 'food_detail_rating_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final ApiService _api = ApiService();
  bool _aksamMi = false;
  DateTime _seciliTarih = DateTime.now();
  late Future<Menu> _menuFuture;

  @override
  void initState() {
    super.initState();
    _menuyuYukle();
  }

  /// Seçili tarihe ve öğüne göre Web API'den menüyü ister.
  void _menuyuYukle() {
    _menuFuture = _api.getGunlukMenu(
      tarih: _seciliTarih,
      ogun: _aksamMi ? 2 : 1,
    );
  }

  void _gunDegistir(int gunFarki) {
    setState(() {
      _seciliTarih = _seciliTarih.add(Duration(days: gunFarki));
      _menuyuYukle();
    });
  }

  Future<void> _tarihSec(BuildContext context) async {
    final secilen = await showDatePicker(
      context: context,
      initialDate: _seciliTarih,
      firstDate: DateTime(2026, 1, 1),
      lastDate: DateTime(2027, 12, 31),
    );
    if (secilen != null) {
      setState(() {
        _seciliTarih = secilen;
        _menuyuYukle();
      });
    }
  }

  String _tarihBicimlendir(DateTime t) {
    const aylar = [
      'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
      'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'
    ];
    return '${t.day} ${aylar[t.month - 1]} ${t.year}';
  }

  @override
  Widget build(BuildContext context) {
    return RefreshIndicator(
      onRefresh: () async {
        setState(_menuyuYukle);
        await _menuFuture;
      },
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          // 1) Gün Gezinme Çubuğu
          Card(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 6),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  IconButton(
                    tooltip: 'Önceki Gün',
                    onPressed: () => _gunDegistir(-1),
                    icon: const Icon(Icons.chevron_left),
                  ),
                  TextButton.icon(
                    onPressed: () => _tarihSec(context),
                    icon: const Icon(Icons.event),
                    label: Text(
                      _tarihBicimlendir(_seciliTarih),
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                  ),
                  IconButton(
                    tooltip: 'Sonraki Gün',
                    onPressed: () => _gunDegistir(1),
                    icon: const Icon(Icons.chevron_right),
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 12),

          // 2) Öğle / Akşam Seçici
          SegmentedButton<bool>(
            segments: const [
              ButtonSegment(value: false, label: Text('☀️ Öğle Menüsü')),
              ButtonSegment(value: true, label: Text('🌙 Akşam Menüsü')),
            ],
            selected: {_aksamMi},
            onSelectionChanged: (secim) => setState(() {
              _aksamMi = secim.first;
              _menuyuYukle();
            }),
          ),
          const SizedBox(height: 16),

          // 3) API'den Gelen Günlük Menü Kartları
          FutureBuilder<Menu>(
            future: _menuFuture,
            builder: (context, snapshot) {
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Padding(
                  padding: EdgeInsets.all(32),
                  child: Center(child: CircularProgressIndicator()),
                );
              }

              if (snapshot.hasError) {
                return _HataKarti(
                  mesaj: 'Menü yüklenemedi. Web API çalışıyor mu?\n${snapshot.error}',
                  onTekrar: () => setState(_menuyuYukle),
                );
              }

              final menu = snapshot.data!;

              // Resmi tatil günü gösterimi
              if (menu.tatilMi) {
                return _TatilKarti(
                  tarihMetni: _tarihBicimlendir(menu.tarih),
                  aciklama: menu.aciklama ?? 'Resmi Tatil – Yemekhane Hizmet Vermemektedir',
                );
              }

              return Column(
                children: [
                  Card(
                    color: Theme.of(context).colorScheme.primaryContainer,
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                _aksamMi ? 'Günlük Akşam Menüsü' : 'Günlük Öğle Menüsü',
                                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                                      fontWeight: FontWeight.bold,
                                    ),
                              ),
                              const SizedBox(height: 4),
                              Text(
                                _tarihBicimlendir(menu.tarih),
                                style: Theme.of(context).textTheme.bodySmall,
                              ),
                            ],
                          ),
                          Chip(
                            avatar: const Icon(Icons.local_fire_department, size: 18),
                            label: Text(
                              menu.toplamKalori != null
                                  ? '${menu.toplamKalori} kcal'
                                  : 'Kalori Bilgisi Yok',
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 8),
                  ...menu.yemekler.map((yemek) => _YemekKarti(yemek: yemek)),
                ],
              );
            },
          ),
        ],
      ),
    );
  }
}

class _TatilKarti extends StatelessWidget {
  final String tarihMetni;
  final String aciklama;

  const _TatilKarti({
    required this.tarihMetni,
    required this.aciklama,
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      color: Colors.amber.shade50,
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          children: [
            const Icon(Icons.celebration, size: 48, color: Colors.deepOrange),
            const SizedBox(height: 12),
            Text(
              'Resmi Tatil Günü',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 4),
            Text(tarihMetni, style: Theme.of(context).textTheme.bodyMedium),
            const SizedBox(height: 12),
            Text(
              aciklama,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.titleMedium?.copyWith(
                    color: Colors.deepOrange.shade800,
                  ),
            ),
          ],
        ),
      ),
    );
  }
}

class _YemekKarti extends StatelessWidget {
  final Yemek yemek;
  const _YemekKarti({required this.yemek});

  IconData _kategoriIkonu(String kategori) {
    final k = kategori.toLowerCase();
    if (k.contains('çorba')) return Icons.soup_kitchen;
    if (k.contains('ana')) return Icons.dinner_dining;
    if (k.contains('yardımcı')) return Icons.rice_bowl;
    return Icons.icecream;
  }

  @override
  Widget build(BuildContext context) {
    return Card(
      child: ListTile(
        leading: CircleAvatar(
          child: Icon(_kategoriIkonu(yemek.kategoriAdi)),
        ),
        title: Text(yemek.ad),
        subtitle: Text(
          yemek.kalori != null
              ? '${yemek.kategoriAdi} • ${yemek.kalori} kcal'
              : yemek.kategoriAdi,
        ),
        trailing: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.star, color: Colors.amber, size: 18),
            const SizedBox(width: 4),
            Text(yemek.ortalamaPuan?.toStringAsFixed(1) ?? '-'),
            const Icon(Icons.chevron_right),
          ],
        ),
        onTap: () => Navigator.of(context).push(
          MaterialPageRoute(builder: (_) => FoodDetailRatingScreen(yemek: yemek)),
        ),
      ),
    );
  }
}

class _HataKarti extends StatelessWidget {
  final String mesaj;
  final VoidCallback onTekrar;
  const _HataKarti({required this.mesaj, required this.onTekrar});

  @override
  Widget build(BuildContext context) {
    return Card(
      color: Theme.of(context).colorScheme.errorContainer,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          children: [
            const Icon(Icons.cloud_off, size: 40),
            const SizedBox(height: 8),
            Text(mesaj, textAlign: TextAlign.center),
            const SizedBox(height: 8),
            FilledButton.icon(
              onPressed: onTekrar,
              icon: const Icon(Icons.refresh),
              label: const Text('Tekrar Dene'),
            ),
          ],
        ),
      ),
    );
  }
}
