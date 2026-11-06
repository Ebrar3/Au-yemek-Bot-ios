import 'dart:convert';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';
import '../models/kategori.dart';
import '../models/menu.dart';
import '../models/yemek.dart';

/// Web API ile konuşan tek sınıf. Ekranlar veritabanına değil, buraya bağlanır.
class ApiService {
  final http.Client _client;

  ApiService({http.Client? client}) : _client = client ?? http.Client();

  static const _zamanAsimi = Duration(seconds: 10);

  /// Ortak GET yardımcı metodu: hata kodlarını tek yerde ele alır.
  Future<dynamic> _get(String yol) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}$yol');
    final response = await _client.get(uri).timeout(_zamanAsimi);
    if (response.statusCode == 200) {
      return jsonDecode(utf8.decode(response.bodyBytes));
    }
    throw Exception('Sunucu hatası (HTTP ${response.statusCode})');
  }

  /// GET /api/kategoriler
  Future<List<Kategori>> getKategoriler() async {
    final data = await _get('/kategoriler') as List<dynamic>;
    return data.map((e) => Kategori.fromJson(e as Map<String, dynamic>)).toList();
  }

  /// GET /api/yemekler (isteğe bağlı kategori filtresi)
  Future<List<Yemek>> getYemekler({int? kategoriId}) async {
    final filtre = kategoriId != null ? '?kategoriId=$kategoriId' : '';
    final data = await _get('/yemekler$filtre') as List<dynamic>;
    return data.map((e) => Yemek.fromJson(e as Map<String, dynamic>)).toList();
  }

  /// GET /api/menuler/bugun?ogun=1&tarih=YYYY-MM-DD
  /// Seçili tarihin ve öğünün menüsünü Menu modeli olarak döndürür.
  Future<Menu> getGunlukMenu({DateTime? tarih, int ogun = 1}) async {
    var sorgu = '/menuler/bugun?ogun=$ogun';
    if (tarih != null) {
      final tarihStr =
          '${tarih.year.toString().padLeft(4, '0')}-${tarih.month.toString().padLeft(2, '0')}-${tarih.day.toString().padLeft(2, '0')}';
      sorgu += '&tarih=$tarihStr';
    }
    final data = await _get(sorgu) as Map<String, dynamic>;
    return Menu.fromJson(data);
  }

  /// GET /api/menuler?yil=2026&ay=10&ogun=1
  /// Aylık yemek takvimi ekranı için seçili ayın tüm menülerini getirir.
  Future<List<Menu>> getAylikMenuler({
    required int yil,
    required int ay,
    int ogun = 1,
  }) async {
    try {
      final data = await _get('/menuler?yil=$yil&ay=$ay&ogun=$ogun') as List<dynamic>;
      return data.map((e) => Menu.fromJson(e as Map<String, dynamic>)).toList();
    } catch (_) {
      // Eğer /api/menuler uç noktası henüz boşsa veya erişilemezse bugünün menüsünü yedek olarak döndür
      final bugun = await getGunlukMenu(ogun: ogun);
      return [bugun];
    }
  }

  /// GET /api/menuler/bugun?ogun=1 (1: Öğle, 2: Akşam)
  Future<Map<String, dynamic>> getBugununMenusu({int ogun = 1}) async {
    return await _get('/menuler/bugun?ogun=$ogun') as Map<String, dynamic>;
  }

  /// GET /api/bot/adim
  Future<Map<String, dynamic>> getBotAdim({String secenek = 'bugun_ogle'}) async {
    return await _get('/bot/adim?secenek=$secenek') as Map<String, dynamic>;
  }
}
