# Grão

Sandbox de areia ("falling sand") em Godot 4 + C#: pós, líquidos, gases, calor, eletricidade, reator nuclear, explosivos, pessoas, animais e vírus. Cada pixel é uma célula com material, temperatura e corrente.

![](https://img.shields.io/badge/Godot-4.7_.NET-478cbf) ![](https://img.shields.io/badge/licença-MIT-green)

## O que tem

- **~90 materiais** em separadores: pós, líquidos, gases, sólidos, eletricidade, explosivos, nuclear, vida e especiais.
- **Calor a sério**: condução por material, capacidade térmica, convecção nos líquidos, mudanças de estado (gelo ↔ água ↔ vapor, pedra ↔ lava, cera, metais que derretem), auto-ignição.
- **Eletricidade em corrente contínua**: bateria, gerador a vapor, painel solar, fios, interruptores, termóstato, resistências que aquecem, lâmpadas, néon, campainha. A água salgada conduz bem e a doce mal; a corrente faz eletrólise (hidrogénio + oxigénio, ou cloro na água salgada) e detona C4.
- **Nuclear**: neutrões rápidos e lentos, urânio, plutónio, moderadores (água, grafite), barras de controlo, fusão do núcleo (cório), bomba atómica.
- **Explosivos**: pólvora, nitroglicerina, C4, termite, dinamite com rastilho, minas, bomba de gelo, fogo de artifício, antimatéria.
- **Vida**: pessoas que andam, sobem degraus, afogam-se e morrem de calor, frio, ácido, cloro, choques e radiação; peixes, minhocas, pássaros, plantas e sementes; vírus que infeta e cura que o desfaz.
- **Sons** gerados por código (sem ficheiros): explosões, trovão, faíscas, fogo, água a chiar, campainha e contador Geiger.
- Ferramentas: borracha, aquecer, arrefecer, faísca, neutrões, raio. Vista de calor (tecla H).
- Teste de CPU embutido (benchmark de 1 thread e stress multi-core).

## Jogar

Precisa do [Godot 4.7 .NET](https://godotengine.org/download) e do .NET 9 SDK.

```bash
./jogar.sh            # compila e abre
./editar.sh           # abre no editor do Godot
GODOT=/caminho/para/godot ./jogar.sh   # se o Godot estiver noutro sítio
```

### Android

```bash
./apk.sh              # gera export/grao.apk (arm64) e instala-o se houver um telemóvel ligado por USB
```

Precisa dos export templates do Godot 4.7.2 .NET, do Android SDK (platform-tools e build-tools), de um JDK 17 e do .NET 9 SDK. A keystore de assinatura fica fora do repositório (por omissão em `~/.local/share/grao/`; muda com `GRAO_KEYSTORE`, `GRAO_KEYSTORE_USER` e `GRAO_KEYSTORE_PASSWORD`). No telemóvel a interface fica maior e o ecrã fica deitado; para apagar usa a Borracha (em Ferramentas).

Controlos no PC: botão esquerdo pinta, direito apaga, roda muda o pincel, Espaço pausa, H vista de calor, M som.

## Como funciona

- `Scripts/Sim.cs` — motor (CPU): grelha de células em chunks de 16×16 que dormem quando nada mexe; calor, corrente (BFS a partir das fontes em cada passo), neutrões como partículas e pessoas como entidades por cima da grelha.
- `Scripts/MatData.cs` — tabela de materiais (propriedades físicas, cores, descrições).
- `world.gdshader` — desenha o mundo na GPU a partir de duas texturas (células e temperatura).
- `Scripts/Sons.cs` — síntese dos sons.

## Créditos

Inspirado no género "falling sand" (Powder Toy, Noita e companhia) e em particular no [Sandboxels](https://sandboxels.r74n.com/) da R74n, de onde tirei ideias de materiais e reações. **Nenhum código do Sandboxels foi usado**: tudo aqui foi escrito de raiz.

## Licença

MIT — vê [LICENSE](LICENSE).
