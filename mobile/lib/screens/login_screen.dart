import 'package:flutter/material.dart';
import '../models/auth_kullanici.dart';
import '../services/auth_service.dart';
import 'register_screen.dart';

/// Kullanıcı Giriş (Login) ve JWT Oturum Bilgisi Ekranı (4. Hafta Görevi).
class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();

  bool _yukleniyor = false;
  bool _sifreGizli = true;
  String? _hataMesaji;
  String? _dogrulamaMesaji;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _girisYap() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _yukleniyor = true;
      _hataMesaji = null;
      _dogrulamaMesaji = null;
    });

    try {
      final kullanici = await AuthService.instance.girisYap(
        eposta: _emailController.text,
        sifre: _passwordController.text,
      );

      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Hoş geldiniz, ${kullanici.adSoyad} (${kullanici.rol})'),
        ),
      );
      setState(() {});
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

  Future<void> _tokenDogrula() async {
    try {
      final sonuc = await AuthService.instance.oturumuDogrula();
      if (!mounted) return;
      setState(() {
        _dogrulamaMesaji =
            'GET /api/auth/me doğrulandı: ${sonuc['adSoyad']} (${sonuc['rol']})';
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _dogrulamaMesaji = 'Doğrulama hatası: $e';
      });
    }
  }

  void _ornekHesapDoldur({required String eposta, required String sifre}) {
    setState(() {
      _emailController.text = eposta;
      _passwordController.text = sifre;
      _hataMesaji = null;
    });
  }

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: AuthService.instance,
      builder: (context, _) {
        final aktif = AuthService.instance.aktifKullanici;
        if (aktif != null) {
          return _AktifOturumGorunumu(
            kullanici: aktif,
            dogrulamaMesaji: _dogrulamaMesaji,
            onDogrula: _tokenDogrula,
            onCikis: () {
              AuthService.instance.cikisYap();
              setState(() => _dogrulamaMesaji = null);
            },
          );
        }

        return ListView(
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
                        'Kullanıcı Girişi (JWT Bearer)',
                        style: Theme.of(context).textTheme.titleLarge,
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Web API (/api/auth/login) üzerinden kimlik doğrulaması yapar ve dönen JWT token ile rol bilgisini saklar.',
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
                        controller: _emailController,
                        keyboardType: TextInputType.emailAddress,
                        decoration: const InputDecoration(
                          labelText: 'E-posta Adresi',
                          prefixIcon: Icon(Icons.email_outlined),
                          border: OutlineInputBorder(),
                        ),
                        validator: (val) {
                          if (val == null || val.trim().isEmpty || !val.contains('@')) {
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
                          labelText: 'Şifre',
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
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),
                      FilledButton.icon(
                        onPressed: _yukleniyor ? null : _girisYap,
                        icon: _yukleniyor
                            ? const SizedBox(
                                width: 18,
                                height: 18,
                                child: CircularProgressIndicator(strokeWidth: 2),
                              )
                            : const Icon(Icons.login),
                        label: Text(_yukleniyor ? 'Giriş Yapılıyor...' : 'Giriş Yap'),
                      ),
                      const SizedBox(height: 8),
                      OutlinedButton.icon(
                        onPressed: () async {
                          await Navigator.of(context).push(
                            MaterialPageRoute(builder: (_) => const RegisterScreen()),
                          );
                          if (mounted) setState(() {});
                        },
                        icon: const Icon(Icons.person_add_alt_1),
                        label: const Text('Hesabın Yok mu? Yeni Kayıt Oluştur'),
                      ),
                    ],
                  ),
                ),
              ),
            ),
            const SizedBox(height: 12),

            // 1. Sınıf İçi Gösterim (18 Kasım) Hızlı Demo Hesapları
            Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Hızlı Demo Hesapları (Rol Testi)',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 8),
                    Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: [
                        ActionChip(
                          avatar: const Icon(Icons.admin_panel_settings, size: 18),
                          label: const Text('Yönetici Hesabını Doldur'),
                          onPressed: () => _ornekHesapDoldur(
                            eposta: 'admin@ankara.edu.tr',
                            sifre: 'Admin123!',
                          ),
                        ),
                        ActionChip(
                          avatar: const Icon(Icons.school, size: 18),
                          label: const Text('Öğrenci Hesabını Doldur'),
                          onPressed: () => _ornekHesapDoldur(
                            eposta: 'ogrenci@ankara.edu.tr',
                            sifre: 'Ogrenci123!',
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
          ],
        );
      },
    );
  }
}

class _AktifOturumGorunumu extends StatelessWidget {
  final AuthKullanici kullanici;
  final String? dogrulamaMesaji;
  final VoidCallback onDogrula;
  final VoidCallback onCikis;

  const _AktifOturumGorunumu({
    required this.kullanici,
    required this.dogrulamaMesaji,
    required this.onDogrula,
    required this.onCikis,
  });

  @override
  Widget build(BuildContext context) {
    final tokenOnizleme = kullanici.token.length > 48
        ? '${kullanici.token.substring(0, 48)}...'
        : kullanici.token;

    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Card(
          color: Theme.of(context).colorScheme.primaryContainer,
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    CircleAvatar(
                      child: Icon(
                        kullanici.yoneticiMi
                            ? Icons.admin_panel_settings
                            : Icons.person,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            kullanici.adSoyad,
                            style: Theme.of(context).textTheme.titleLarge,
                          ),
                          Text(kullanici.eposta),
                        ],
                      ),
                    ),
                    Chip(
                      label: Text(kullanici.rol),
                      backgroundColor: kullanici.yoneticiMi
                          ? Colors.amber.shade200
                          : Colors.green.shade100,
                    ),
                  ],
                ),
                const Divider(height: 24),
                Text(
                  'Saklanan JWT Bearer Token:',
                  style: Theme.of(context).textTheme.labelLarge,
                ),
                const SizedBox(height: 4),
                SelectableText(
                  tokenOnizleme,
                  style: const TextStyle(fontFamily: 'monospace', fontSize: 12),
                ),
                const SizedBox(height: 12),
                if (dogrulamaMesaji != null) ...[
                  Text(
                    dogrulamaMesaji!,
                    style: const TextStyle(fontWeight: FontWeight.w600),
                  ),
                  const SizedBox(height: 8),
                ],
                Row(
                  children: [
                    Expanded(
                      child: FilledButton.tonalIcon(
                        onPressed: onDogrula,
                        icon: const Icon(Icons.verified_user_outlined),
                        label: const Text('/api/auth/me Doğrula'),
                      ),
                    ),
                    const SizedBox(width: 8),
                    OutlinedButton.icon(
                      onPressed: onCikis,
                      icon: const Icon(Icons.logout),
                      label: const Text('Çıkış Yap'),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }
}
