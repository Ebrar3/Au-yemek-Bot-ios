import 'package:flutter_test/flutter_test.dart';
import 'package:au_bot_mobile/main.dart';

void main() {
  testWidgets('Aü Bot ana ekran testi', (WidgetTester tester) async {
    await tester.pumpWidget(const AuBotApp());
    expect(find.text('Aü Bot – Yemekhane Asistanı'), findsOneWidget);
  });
}
