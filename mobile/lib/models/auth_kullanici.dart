class AuthKullanici {
  final String token;
  final DateTime sonGecerlilik;
  final String kullaniciId;
  final String adSoyad;
  final String eposta;
  final String rol;
  final List<String> roller;
  final bool yoneticiMi;
  final String mesaj;

  const AuthKullanici({
    required this.token,
    required this.sonGecerlilik,
    required this.kullaniciId,
    required this.adSoyad,
    required this.eposta,
    required this.rol,
    required this.roller,
    required this.yoneticiMi,
    required this.mesaj,
  });

  /// Web API (/api/auth/login ve /api/auth/register) yanıtını modele çevirir.
  factory AuthKullanici.fromJson(Map<String, dynamic> json) {
    final rollerListesi = (json['roller'] as List<dynamic>?)
            ?.map((e) => e.toString())
            .toList() ??
        const [];

    return AuthKullanici(
      token: json['token'] as String? ?? '',
      sonGecerlilik: DateTime.tryParse(json['sonGecerlilik']?.toString() ?? '') ??
          DateTime.now().add(const Duration(hours: 24)),
      kullaniciId: json['kullaniciId'] as String? ?? '',
      adSoyad: json['adSoyad'] as String? ?? '',
      eposta: json['eposta'] as String? ?? '',
      rol: json['rol'] as String? ?? 'Öğrenci',
      roller: rollerListesi,
      yoneticiMi: json['yoneticiMi'] as bool? ?? false,
      mesaj: json['mesaj'] as String? ?? '',
    );
  }

  Map<String, dynamic> toJson() => {
        'token': token,
        'sonGecerlilik': sonGecerlilik.toIso8601String(),
        'kullaniciId': kullaniciId,
        'adSoyad': adSoyad,
        'eposta': eposta,
        'rol': rol,
        'roller': roller,
        'yoneticiMi': yoneticiMi,
        'mesaj': mesaj,
      };
}
