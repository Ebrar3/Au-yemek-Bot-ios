import 'package:flutter/material.dart';

class BotScreen extends StatefulWidget {
  const BotScreen({super.key});

  @override
  State<BotScreen> createState() => _BotScreenState();
}

class _BotScreenState extends State<BotScreen> {
  String _aktifBaslik = 'Bugünün Öğle Menüsü';
  String _aktifMesaj =
      '• Mercimek Çorbası (180 kcal)\n'
      '• Orman Kebabı (420 kcal)\n'
      '• Pirinç Pilavı (310 kcal)\n'
      '• Ayran & Mevsim Salata (210 kcal)\n\n'
      'Toplam: 1120 kcal';

  void _secenekSec(String kod) {
    setState(() {
      switch (kod) {
        case 'aksam':
          _aktifBaslik = 'Bugünün Akşam Menüsü';
          _aktifMesaj =
              '• Ezogelin Çorbası (190 kcal)\n'
              '• Tavuk Sote (380 kcal)\n'
              '• Bulgur Pilavı (290 kcal)\n'
              '• Kemalpaşa Tatlısı (310 kcal)';
          break;
        case 'ertesi_gun':
          _aktifBaslik = 'Ertesi Günün Öğle Menüsü';
          _aktifMesaj =
              '• Yayla Çorbası (170 kcal)\n'
              '• İzmir Köfte (440 kcal)\n'
              '• Makarna (320 kcal)\n'
              '• Meyve (110 kcal)';
          break;
        case 'puanlama':
          _aktifBaslik = 'Hızlı Yemek Puanlama';
          _aktifMesaj = 'Menü sekmesinden veya yemek kartına dokunarak 1–5 arası puan verebilirsiniz.';
          break;
        case 'yardim':
          _aktifBaslik = 'Yardım & İletişim';
          _aktifMesaj = 'Öneri ve hata bildirimleriniz için: aubot.destek@ankara.edu.tr';
          break;
        default:
          _aktifBaslik = 'Bugünün Öğle Menüsü';
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(_aktifBaslik, style: Theme.of(context).textTheme.titleLarge),
                  const Divider(),
                  Text(_aktifMesaj),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
          Text('Seçenekler', style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: 8),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              FilledButton.tonal(
                onPressed: () => _secenekSec('puanlama'),
                child: const Text('⭐ Puanlama'),
              ),
              FilledButton.tonal(
                onPressed: () => _secenekSec('aksam'),
                child: const Text('🌙 Akşam Yemeği'),
              ),
              FilledButton.tonal(
                onPressed: () => _secenekSec('ertesi_gun'),
                child: const Text('⏭️ Ertesi Günün Öğle Yemeği'),
              ),
              FilledButton.tonal(
                onPressed: () => _secenekSec('bugun_ogle'),
                child: const Text('⏮️ Bugünün Öğle Yemeği'),
              ),
              FilledButton.tonal(
                onPressed: () => _secenekSec('yardim'),
                child: const Text('🆘 Yardım'),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
