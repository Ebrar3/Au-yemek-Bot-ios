import 'dart:convert';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';
import '../models/kategori.dart';
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

  /// GET /api/menuler/bugun?ogun=1 (1: Öğle, 2: Akşam)
  Future<Map<String, dynamic>> getBugununMenusu({int ogun = 1}) async {
    return await _get('/menuler/bugun?ogun=$ogun') as Map<String, dynamic>;
  }

  /// GET /api/bot/adim
  Future<Map<String, dynamic>> getBotAdim({String secenek = 'bugun_ogle'}) async {
    return await _get('/bot/adim?secenek=$secenek') as Map<String, dynamic>;
  }
}
