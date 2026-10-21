class Kategori {
  final int id;
  final String ad;
  final int sira;

  const Kategori({
    required this.id,
    required this.ad,
    required this.sira,
  });

  factory Kategori.fromJson(Map<String, dynamic> json) {
    return Kategori(
      id: json['id'] as int,
      ad: json['ad'] as String,
      sira: json['sira'] as int? ?? 1,
    );
  }
}
