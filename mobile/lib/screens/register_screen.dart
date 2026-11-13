import 'package:flutter/material.dart';
import '../services/auth_service.dart';

/// Kullanıcı Kayıt (Register) Ekranı (4. Hafta Görevi).
/// Form doğrulama (validation) kurallarını çalıştırır ve POST /api/auth/register
/// üzerinden yeni öğrenci hesabı oluşturup dönen JWT token'ı saklar.
class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _adSoyadController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _passwordConfirmController = TextEditingController();

  bool _yukleniyor = false;
  bool _sifreGizli = true;
  String? _hataMesaji;

  @override
  void dispose() {
    _adSoyadController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _passwordConfirmController.dispose();
    super.dispose();
  }

  Future<void> _kayitOl() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _yukleniyor = true;
      _hataMesaji = null;
    });

    try {
      final kullanici = await AuthService.instance.kayitOl(
        adSoyad: _adSoyadController.text,
        eposta: _emailController.text,
        sifre: _passwordController.text,
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('${kullanici.adSoyad} olarak kayıt olundu (${kullanici.rol}).'),
        ),
      );
      Navigator.of(context).pop(true);
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _hataMesaji = e.toString().replaceFirst('Exception: ', '');
      });
    } finally {
      if (mounted) {
        setState(() => _yukleniyor = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Yeni Öğrenci Kaydı'),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(
                      'Aü Bot Hesabı Oluştur',
                      style: Theme.of(context).textTheme.titleLarge,
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Kayıt olan kullanıcılara varsayılan olarak "Öğrenci" rolü atanır ve JWT token üretilir.',
                      style: Theme.of(context).textTheme.bodySmall,
                    ),
                    const SizedBox(height: 16),
                    if (_hataMesaji != null) ...[
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Theme.of(context).colorScheme.errorContainer,
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(
                          _hataMesaji!,
                          style: TextStyle(
                            color: Theme.of(context).colorScheme.onErrorContainer,
                          ),
                        ),
                      ),
                      const SizedBox(height: 12),
                    ],
                    TextFormField(
                      controller: _adSoyadController,
                      decoration: const InputDecoration(
                        labelText: 'Ad Soyad',
                        prefixIcon: Icon(Icons.badge_outlined),
                        border: OutlineInputBorder(),
                      ),
                      validator: (val) {
                        if (val == null || val.trim().length < 2) {
                          return 'Ad Soyad en az 2 karakter olmalıdır.';
                        }
                        if (val.trim().length > 100) {
                          return 'Ad Soyad en fazla 100 karakter olabilir.';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _emailController,
                      keyboardType: TextInputType.emailAddress,
                      decoration: const InputDecoration(
                        labelText: 'Üniversite E-posta Adresi',
                        hintText: 'ornek@ankara.edu.tr',
                        prefixIcon: Icon(Icons.email_outlined),
                        border: OutlineInputBorder(),
                      ),
                      validator: (val) {
                        if (val == null || val.trim().isEmpty) {
                          return 'E-posta adresi zorunludur.';
                        }
                        final temiz = val.trim();
                        if (!temiz.contains('@') || !temiz.contains('.')) {
                          return 'Geçerli bir e-posta adresi giriniz.';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _passwordController,
                      obscureText: _sifreGizli,
                      decoration: InputDecoration(
                        labelText: 'Şifre (en az 6 karakter, harf ve rakam)',
                        prefixIcon: const Icon(Icons.lock_outline),
                        border: const OutlineInputBorder(),
                        suffixIcon: IconButton(
                          icon: Icon(
                            _sifreGizli ? Icons.visibility_off : Icons.visibility,
                          ),
                          onPressed: () => setState(() => _sifreGizli = !_sifreGizli),
                        ),
                      ),
                      validator: (val) {
                        if (val == null || val.length < 6) {
                          return 'Şifre en az 6 karakter olmalıdır.';
                        }
                        if (!RegExp(r'[a-z]').hasMatch(val)) {
                          return 'Şifre en az bir küçük harf içermelidir.';
                        }
                        if (!RegExp(r'[0-9]').hasMatch(val)) {
                          return 'Şifre en az bir rakam içermelidir.';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _passwordConfirmController,
                      obscureText: _sifreGizli,
                      decoration: const InputDecoration(
                        labelText: 'Şifre Tekrar',
                        prefixIcon: Icon(Icons.lock_reset),
                        border: OutlineInputBorder(),
                      ),
                      validator: (val) {
                        if (val != _passwordController.text) {
                          return 'Şifreler birbiriyle eşleşmiyor.';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 16),
                    FilledButton.icon(
                      onPressed: _yukleniyor ? null : _kayitOl,
                      icon: _yukleniyor
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Icon(Icons.person_add),
                      label: Text(_yukleniyor ? 'Kayıt Yapılıyor...' : 'Kayıt Ol ve Giriş Yap'),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
