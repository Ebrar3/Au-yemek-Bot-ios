import 'yemek.dart';

enum OgunTuru { ogle, aksam }

class Menu {
  final int id;
  final DateTime tarih;
  final OgunTuru ogun;
  final int? toplamKalori;
  final bool tatilMi;
  final String? aciklama;
  final List<Yemek> yemekler;

  const Menu({
    required this.id,
    required this.tarih,
    required this.ogun,
    this.toplamKalori,
    this.tatilMi = false,
    this.aciklama,
    this.yemekler = const [],
  });
}
