import 'package:flutter/material.dart';
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
  late Future<Map<String, dynamic>> _menuFuture;

  @override
  void initState() {
    super.initState();
    _menuyuYukle();
  }

  /// Seçili öğüne göre API'den menüyü (yeniden) ister.
  void _menuyuYukle() {
    _menuFuture = _api.getBugununMenusu(ogun: _aksamMi ? 2 : 1);
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
          FutureBuilder<Map<String, dynamic>>(
            future: _menuFuture,
            builder: (context, snapshot) {
              // 1) Yükleniyor durumu
              if (snapshot.connectionState == ConnectionState.waiting) {
                return const Padding(
                  padding: EdgeInsets.all(32),
                  child: Center(child: CircularProgressIndicator()),
                );
              }
              // 2) Hata durumu (API kapalı, internet yok vb.)
              if (snapshot.hasError) {
                return _HataKarti(
                  mesaj: 'Menü yüklenemedi. Web API çalışıyor mu?\n${snapshot.error}',
                  onTekrar: () => setState(_menuyuYukle),
                );
              }
              // 3) Başarılı durum
              final data = snapshot.data!;
              final yemekler = (data['yemekler'] as List<dynamic>)
                  .map((e) => Yemek.fromJson(e as Map<String, dynamic>))
                  .toList();
              return Column(
                children: [
                  Card(
                    child: Padding(
                      padding: const EdgeInsets.all(16),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Text(
                            _aksamMi ? 'Bugünün Akşam Menüsü' : 'Bugünün Öğle Menüsü',
                            style: Theme.of(context).textTheme.titleMedium,
                          ),
                          Chip(label: Text('${data['toplamKalori'] ?? '-'} kcal')),
                        ],
                      ),
                    ),
                  ),
                  const SizedBox(height: 8),
                  ...yemekler.map((yemek) => _YemekKarti(yemek: yemek)),
                ],
              );
            },
          ),
        ],
      ),
    );
  }
}

class _YemekKarti extends StatelessWidget {
  final Yemek yemek;
  const _YemekKarti({required this.yemek});

  @override
  Widget build(BuildContext context) {
    return Card(
      child: ListTile(
        title: Text(yemek.ad),
        subtitle: Text('${yemek.kategoriAdi} • ${yemek.kalori ?? '-'} kcal'),
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
