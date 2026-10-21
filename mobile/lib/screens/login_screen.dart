import 'package:flutter/material.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Form(
        key: _formKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text('Kullanıcı Girişi (JWT)', style: Theme.of(context).textTheme.titleLarge),
            const SizedBox(height: 16),
            TextFormField(
              controller: _emailController,
              decoration: const InputDecoration(
                labelText: 'E-posta Adresi',
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
              obscureText: true,
              decoration: const InputDecoration(
                labelText: 'Şifre',
                border: OutlineInputBorder(),
              ),
              validator: (val) {
                if (val == null || val.length < 6) {
                  return 'Şifre en az 6 karakter olmalıdır.';
                }
                return null;
              },
            ),
            const SizedBox(height: 16),
            FilledButton(
              onPressed: () {
                if (_formKey.currentState!.validate()) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Giriş isteği gönderiliyor...')),
                  );
                }
              },
              child: const Text('Giriş Yap'),
            ),
          ],
        ),
      ),
    );
  }
}
