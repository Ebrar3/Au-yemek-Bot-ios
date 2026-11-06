import 'yemek.dart';

enum OgunTuru {
  ogle(1, 'Öğle'),
  aksam(2, 'Akşam');

  final int deger;
  final String etiket;
  const OgunTuru(this.deger, this.etiket);

  static OgunTuru fromValue(dynamic val) {
    if (val == 2 || val?.toString().toLowerCase() == 'aksam') {
      return OgunTuru.aksam;
    }
    return OgunTuru.ogle;
  }
}

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

  /// Web API'den gelen günlük menü JSON nesnesini Menu modeline dönüştürür.
  factory Menu.fromJson(Map<String, dynamic> json) {
    final tarihStr = json['tarih']?.toString() ?? '';
    final parsedTarih = DateTime.tryParse(tarihStr) ?? DateTime.now();

    final hamYemekler = (json['yemekler'] as List<dynamic>?) ?? const [];
    final yemekListesi = hamYemekler
        .map((e) => Yemek.fromJson(e as Map<String, dynamic>))
        .toList();

    return Menu(
      id: json['id'] as int? ?? 0,
      tarih: parsedTarih,
      ogun: OgunTuru.fromValue(json['ogun']),
      toplamKalori: json['toplamKalori'] as int?,
      tatilMi: json['tatilMi'] as bool? ?? false,
      aciklama: json['aciklama'] as String?,
      yemekler: yemekListesi,
    );
  }
}
