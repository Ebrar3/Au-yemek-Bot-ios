class Yemek {
  final int id;
  final String ad;
  final int? kategoriId;
  final String kategoriAdi;
  final int? kalori;
  final double? ortalamaPuan;

  const Yemek({
    required this.id,
    required this.ad,
    this.kategoriId,
    required this.kategoriAdi,
    this.kalori,
    this.ortalamaPuan,
  });

  /// API'den gelen JSON'u Dart nesnesine çevirir.
  /// /api/yemekler "kategoriAdi", /api/menuler/bugun "kategori" alanını döndürür.
  factory Yemek.fromJson(Map<String, dynamic> json) {
    return Yemek(
      id: json['id'] as int? ?? 0,
      ad: json['ad'] as String? ?? '',
      kategoriId: json['kategoriId'] as int?,
      kategoriAdi: (json['kategoriAdi'] ?? json['kategori'] ?? '') as String,
      kalori: json['kalori'] as int?,
      ortalamaPuan: (json['ortalamaPuan'] as num?)?.toDouble(),
    );
  }
}
