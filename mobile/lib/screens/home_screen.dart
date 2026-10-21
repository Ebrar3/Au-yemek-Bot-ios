import 'package:flutter/material.dart';
import '../models/yemek.dart';
import 'food_detail_rating_screen.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  bool _aksamMi = false;

  final List<Yemek> _ornekYemekler = const [
    Yemek(id: 1, ad: 'Mercimek Çorbası', kategoriAdi: 'Çorba', kalori: 180, ortalamaPuan: 4.6),
    Yemek(id: 2, ad: 'Orman Kebabı', kategoriAdi: 'Ana Yemek', kalori: 420, ortalamaPuan: 4.4),
    Yemek(id: 3, ad: 'Pirinç Pilavı', kategoriAdi: 'Yardımcı Yemek', kalori: 310, ortalamaPuan: 4.2),
    Yemek(id: 4, ad: 'Ayran & Mevsim Salata', kategoriAdi: 'Tatlı / Salata', kalori: 210, ortalamaPuan: 4.5),
  ];

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        SegmentedButton<bool>(
          segments: const [
            ButtonSegment(value: false, label: Text('☀️ Öğle Menüsü')),
            ButtonSegment(value: true, label: Text('🌙 Akşam Menüsü')),
          ],
          selected: {_aksamMi},
          onSelectionChanged: (secim) => setState(() => _aksamMi = secim.first),
        ),
        const SizedBox(height: 16),
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
                const Chip(label: Text('1120 kcal')),
              ],
            ),
          ),
        ),
        const SizedBox(height: 8),
        ..._ornekYemekler.map(
          (yemek) => Card(
            child: ListTile(
              title: Text(yemek.ad),
              subtitle: Text('${yemek.kategoriAdi} • ${yemek.kalori} kcal'),
              trailing: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Icon(Icons.star, color: Colors.amber, size: 18),
                  const SizedBox(width: 4),
                  Text('${yemek.ortalamaPuan ?? '-'}'),
                  const Icon(Icons.chevron_right),
                ],
              ),
              onTap: () {
                Navigator.of(context).push(
                  MaterialPageRoute(
                    builder: (_) => FoodDetailRatingScreen(yemek: yemek),
                  ),
                );
              },
            ),
          ),
        ),
      ],
    );
  }
}
