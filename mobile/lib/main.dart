import 'package:flutter/material.dart';
import 'screens/home_screen.dart';
import 'screens/monthly_calendar_screen.dart';
import 'screens/bot_screen.dart';
import 'screens/popular_foods_screen.dart';
import 'screens/login_screen.dart';
import 'screens/admin_panel_screen.dart';
import 'services/auth_service.dart';

void main() {
  runApp(const AuBotApp());
}

class AuBotApp extends StatelessWidget {
  const AuBotApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Aü Bot',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF1A3C6E)),
        useMaterial3: true,
      ),
      home: const MainShell(),
    );
  }
}

class MainShell extends StatefulWidget {
  const MainShell({super.key});

  @override
  State<MainShell> createState() => _MainShellState();
}

class _MainShellState extends State<MainShell> {
  int _selectedIndex = 0;

  final List<Widget> _screens = const [
    HomeScreen(),
    MonthlyCalendarScreen(),
    BotScreen(),
    PopularFoodsScreen(),
    AdminPanelScreen(),
    LoginScreen(),
  ];

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: AuthService.instance,
      builder: (context, _) {
        final aktif = AuthService.instance.aktifKullanici;
        return Scaffold(
          appBar: AppBar(
            title: const Text('Aü Bot – Yemekhane Asistanı'),
            centerTitle: true,
            actions: [
              if (aktif != null)
                Padding(
                  padding: const EdgeInsets.only(right: 12),
                  child: Chip(
                    label: Text(aktif.rol),
                    visualDensity: VisualDensity.compact,
                  ),
                ),
            ],
          ),
          body: _screens[_selectedIndex],
          bottomNavigationBar: NavigationBar(
            selectedIndex: _selectedIndex,
            onDestinationSelected: (idx) => setState(() => _selectedIndex = idx),
            destinations: [
              const NavigationDestination(icon: Icon(Icons.today), label: 'Günlük'),
              const NavigationDestination(icon: Icon(Icons.calendar_month), label: 'Takvim'),
              const NavigationDestination(icon: Icon(Icons.smart_toy_outlined), label: 'Aü Bot'),
              const NavigationDestination(icon: Icon(Icons.star_outline), label: 'Popüler'),
              const NavigationDestination(icon: Icon(Icons.admin_panel_settings_outlined), label: 'Yönetim'),
              NavigationDestination(
                icon: Icon(aktif != null ? Icons.verified_user : Icons.person_outline),
                label: aktif != null ? 'Profil' : 'Giriş',
              ),
            ],
          ),
        );
      },
    );
  }
}
