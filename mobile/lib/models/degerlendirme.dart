class Degerlendirme {
  final int id;
  final int menuId;
  final int yemekId;
  final int puan;
  final String? yorum;
  final DateTime tarih;

  const Degerlendirme({
    required this.id,
    required this.menuId,
    required this.yemekId,
    required this.puan,
    this.yorum,
    required this.tarih,
  });
}
