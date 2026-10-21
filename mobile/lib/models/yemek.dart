class Yemek {
  final int id;
  final String ad;
  final String kategoriAdi;
  final int? kalori;
  final double? ortalamaPuan;

  const Yemek({
    required this.id,
    required this.ad,
    required this.kategoriAdi,
    this.kalori,
    this.ortalamaPuan,
  });

  factory Yemek.fromJson(Map<String, dynamic> json) {
    return Yemek(
      id: json['id'] as int? ?? 0,
      ad: json['ad'] as String? ?? '',
      kategoriAdi: json['kategori'] as String? ?? '',
      kalori: json['kalori'] as int?,
      ortalamaPuan: (json['ortalamaPuan'] as num?)?.toDouble(),
    );
  }
}
