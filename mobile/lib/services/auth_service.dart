import 'dart:convert';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';
import '../models/auth_kullanici.dart';

/// JWT Bearer kimlik doğrulama, giriş/kayıt istekleri ve token saklama servisi (4. Hafta Görevi).
class AuthService extends ChangeNotifier {
  static final AuthService instance = AuthService._();
  AuthService._();

  final http.Client _client = http.Client();
  static const _zamanAsimi = Duration(seconds: 10);

  AuthKullanici? _aktifKullanici;
  final Map<String, String> _yerelDepo = {};

  AuthKullanici? get aktifKullanici => _aktifKullanici;
  bool get girisYapildiMi => _aktifKullanici != null && _aktifKullanici!.token.isNotEmpty;
  bool get yoneticiMi => _aktifKullanici?.yoneticiMi ?? false;
  String? get jwtToken => _aktifKullanici?.token;

  /// Yetkili API istekleri için `Authorization: Bearer <token>` başlığını üretir.
  Map<String, String> get yetkiBasliklari {
    final basliklar = <String, String>{
      'Content-Type': 'application/json; charset=utf-8',
    };
    if (jwtToken != null && jwtToken!.isNotEmpty) {
      basliklar['Authorization'] = 'Bearer $jwtToken';
    }
    return basliklar;
  }

  /// POST /api/auth/login ile kullanıcı girişi yapar ve dönen JWT token'ı saklar.
  Future<AuthKullanici> girisYap({
    required String eposta,
    required String sifre,
  }) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}/auth/login');
    final response = await _client
        .post(
          uri,
          headers: {'Content-Type': 'application/json; charset=utf-8'},
          body: jsonEncode({
            'eposta': eposta.trim(),
            'sifre': sifre,
          }),
        )
        .timeout(_zamanAsimi);

    final govde = jsonDecode(utf8.decode(response.bodyBytes)) as Map<String, dynamic>;

    if (response.statusCode == 200) {
      final kullanici = AuthKullanici.fromJson(govde);
      _oturumuKaydet(kullanici);
      return kullanici;
    }

    final hataMesaji = govde['mesaj']?.toString() ?? 'Giriş yapılamadı (HTTP ${response.statusCode}).';
    throw Exception(hataMesaji);
  }

  /// POST /api/auth/register ile yeni öğrenci hesabı oluşturur ve JWT token'ı saklar.
  Future<AuthKullanici> kayitOl({
    required String adSoyad,
    required String eposta,
    required String sifre,
  }) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}/auth/register');
    final response = await _client
        .post(
          uri,
          headers: {'Content-Type': 'application/json; charset=utf-8'},
          body: jsonEncode({
            'adSoyad': adSoyad.trim(),
            'eposta': eposta.trim(),
            'sifre': sifre,
          }),
        )
        .timeout(_zamanAsimi);

    final govde = jsonDecode(utf8.decode(response.bodyBytes)) as Map<String, dynamic>;

    if (response.statusCode == 201 || response.statusCode == 200) {
      final kullanici = AuthKullanici.fromJson(govde);
      _oturumuKaydet(kullanici);
      return kullanici;
    }

    final hataMesaji = govde['mesaj']?.toString() ?? 'Kayıt oluşturulamadı (HTTP ${response.statusCode}).';
    throw Exception(hataMesaji);
  }

  /// GET /api/auth/me uç noktasına saklanan JWT Bearer token ile istek atarak oturumu doğrular.
  Future<Map<String, dynamic>> oturumuDogrula() async {
    if (!girisYapildiMi) {
      throw Exception('Aktif oturum bulunamadı.');
    }

    final uri = Uri.parse('${ApiConfig.baseUrl}/auth/me');
    final response = await _client
        .get(uri, headers: yetkiBasliklari)
        .timeout(_zamanAsimi);

    final govde = jsonDecode(utf8.decode(response.bodyBytes)) as Map<String, dynamic>;
    if (response.statusCode == 200) {
      return govde;
    }

    throw Exception(govde['mesaj']?.toString() ?? 'Oturum doğrulanamadı (HTTP ${response.statusCode}).');
  }

  void _oturumuKaydet(AuthKullanici kullanici) {
    _aktifKullanici = kullanici;
    _yerelDepo['jwt_token'] = kullanici.token;
    _yerelDepo['kullanici_json'] = jsonEncode(kullanici.toJson());
    notifyListeners();
  }

  /// Saklanan JWT token ve kullanıcı bilgilerini temizleyerek oturumu kapatır.
  void cikisYap() {
    _aktifKullanici = null;
    _yerelDepo.clear();
    notifyListeners();
  }
}
