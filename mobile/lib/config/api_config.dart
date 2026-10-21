/// Web API bağlantı ayarları.
/// Android emülatör için 10.0.2.2, iOS simülatör için localhost kullanılır.
class ApiConfig {
  static const String androidEmulatorBaseUrl = 'http://10.0.2.2:5000/api';
  static const String iosSimulatorBaseUrl = 'http://localhost:5000/api';

  /// Çalıştırılan ortama göre varsayılan API adresi
  static String baseUrl = iosSimulatorBaseUrl;
}
