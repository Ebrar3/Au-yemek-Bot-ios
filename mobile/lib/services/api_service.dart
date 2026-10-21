import 'dart:convert';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';

class ApiService {
  final http.Client _client;

  ApiService({http.Client? client}) : _client = client ?? http.Client();

  Future<Map<String, dynamic>> getBugununMenusu({int ogun = 1}) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}/menuler/bugun?ogun=$ogun');
    final response = await _client.get(uri);
    if (response.statusCode == 200) {
      return jsonDecode(response.body) as Map<String, dynamic>;
    }
    throw Exception('Menü verisi alınamadı (HTTP ${response.statusCode})');
  }

  Future<Map<String, dynamic>> getBotAdim({String secenek = 'bugun_ogle'}) async {
    final uri = Uri.parse('${ApiConfig.baseUrl}/bot/adim?secenek=$secenek');
    final response = await _client.get(uri);
    if (response.statusCode == 200) {
      return jsonDecode(response.body) as Map<String, dynamic>;
    }
    throw Exception('Bot yanıtı alınamadı (HTTP ${response.statusCode})');
  }
}
