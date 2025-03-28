# Gra Tetris stworzona jako zadanie testowe. Unity, C#.

Tetris został stworzony bez użycia siatki – cała mechanika opiera się wyłącznie na fizyce, co okazało się dość wymagające pod względem implementacji. Przed rozpoczęciem projektu nie miałem doświadczenia z Assembly, nowym systemem Input System, a o eventach wiedziałem głównie z teorii, więc musiałem się sporo nauczyć po drodze. Gra działa, choć występują drobne bugi, których nie zdążyłem jeszcze naprawić. Przykładowo, gdy blok spada obok innego, nadal można przesunąć go w bok i „wjechać” w sąsiedni klocek. Zdarza się też, choć rzadko, że bloki glitchują się w bocznych ścianach. Nie korzystałem z żadnych gotowych tutoriali – pomysły i ogólna koncepcja były w pełni moje. W niektórych miejscach miałem jednak problem z implementacją konkretnych rozwiązań, dlatego wspierałem się AI – głównie przy analizie problemów, sugestiach kodu, formatowaniu i dodawaniu komentarzy. 

- ✔️ Dwóch graczy – jeden gra po lewej, drugi po prawej
- ✔️ Minimalistyczny UI: next klocek, score, nazwa gracza
- ✔️ Tylko 2–3 typy klocków (nie wszystkie)
- ✔️ Klocki przesuwają się płynnie (nie skokowo)
- ✔️ Klocki używają fizyki do ruchu i kolizji
- ✔️ Klocki nie mogą się obracać
- ✔️ Logika gry w osobnej scenie i osobnym assembly
- ✔️ Funkcje, komentarze po angielsku
- ✔️ Kod zgodny z konwencjami Microsoftu (naming, styl)
- ✔️ URP użyty
- ✔️ Unity w wersji 6000.0.42f1
- ✔️ Nowy Input System, stary wyłączony
- ✔️ Kompiluje się w IL2CPP z pełnym code strippingiem
- ❌ Logika + fizyka zrobiona w DOTS (Unity Physics)
- ❌ Multiplayer – host i klient (Netcode for Entities)
- ❌ Voice chat – Vivox

![tetris_gif](https://github.com/user-attachments/assets/cda06caf-236c-4a97-b2ab-f9f3b08de0ba)




