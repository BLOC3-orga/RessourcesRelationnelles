# R2 - (Re)Sources Relationnelles

## Vue d'ensemble

R2 est une application web de gestion de ressources relationnelles développée en .NET 8 avec Blazor Server. Elle permet aux utilisateurs de créer, partager et gérer des ressources pédagogiques dans différentes catégories.

## Architecture

### Structure du projet

```
RessourcesRelationnelles/
├── R2.UI/                  # Interface utilisateur Blazor Server
├── R2.Data/               # Couche de données et entités
├── R2.Tests/              # Tests unitaires et d'intégration
├── .github/workflows/     # Pipelines CI/CD GitHub Actions
└── docker-compose.yml     # Configuration Docker
```

### Technologies utilisées

- **Frontend**: Blazor Server (.NET 8)
- **Backend**: ASP.NET Core 8
- **Base de données**: SQL Server
- **ORM**: Entity Framework Core
- **Authentification**: ASP.NET Core Identity
- **Tests**: xUnit, bUnit, Moq
- **Containerisation**: Docker
- **CI/CD**: GitHub Actions
- **Sécurité**: Snyk

## Modèle de données

### Entités principales

#### User (Utilisateur)
```csharp
public class User : IdentityUser<int>
{
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Pseudo { get; set; }
    public string City { get; set; }
    public string Address { get; set; }
    public bool IsAccountActivated { get; set; }
    
    // Collections de navigation
    public ICollection<Resource>? FavoriteResources { get; set; }
    public ICollection<Resource>? ExploitedResources { get; set; }
    public ICollection<Resource>? DraftResources { get; set; }
    public ICollection<Resource>? CreatedResources { get; set; }
}
```

#### Resource (Ressource)
```csharp
public class Resource
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreationDate { get; set; }
    public ResourceType Type { get; set; }        // Activity, Game, Document
    public ResourceStatus Status { get; set; }    // Private, Public, Draft, Hanged
    public int CategoryId { get; set; }
    public virtual Category? Category { get; set; }
}
```

#### Category (Catégorie)
```csharp
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

#### Comment (Commentaire)
```csharp
public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; }
}
```

#### Progression
```csharp
public class Progression
{
    public int Id { get; set; }
    public float Percentage { get; set; }
    public DateTime LastInteractionDate { get; set; }
    public ProgressionStatus Status { get; set; } // NotStarted, InProgress, Completed
}
```

## Système d'authentification et d'autorisation

### Rôles disponibles
- **Super-Administrateur** : Accès complet au système
- **Administrateur** : Gestion des utilisateurs et ressources
- **Modérateur** : Modération du contenu
- **Citoyen** : Utilisateur standard

### Politique d'autorisation
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("Administrateur", "Super-Administrateur"));
    options.AddPolicy("RequireSuperAdminRole", policy => 
        policy.RequireRole("Super-Administrateur"));
    options.AddPolicy("RequireModeratorRole", policy => 
        policy.RequireRole("Modérateur", "Administrateur", "Super-Administrateur"));
});
```

## Fonctionnalités principales

### 1. Gestion des ressources
- **Création** : Les utilisateurs peuvent créer des ressources (activités, jeux, documents)
- **Classification** : Système de catégories pour organiser les ressources
- **Statuts** : Privé, Public, Brouillon, Suspendu
- **CRUD complet** : Create, Read, Update, Delete avec interface intuitive

### 2. Système de filtrage et tri
- Filtrage par statut, type, et catégorie
- Tri par nom (A-Z, Z-A) et date (récente/ancienne)
- Interface responsive avec filtres avancés

### 3. Authentification et autorisation
- Inscription/connexion sécurisée
- Gestion des rôles hiérarchiques
- Activation/désactivation de comptes

### 4. Interface utilisateur
- **Desktop** : Sidebar avec menu de navigation
- **Mobile** : Barre de navigation horizontale en bas d'écran
- Design responsive avec Bootstrap

## Architecture des composants

### Structure des pages

```
R2.UI/Components/Pages/
├── AuthPages/           # Authentification
│   ├── Login.razor
│   ├── Register.razor
│   └── Logout.razor
├── ResourcePages/       # Gestion des ressources
│   ├── Create.razor
│   ├── Edit.razor
│   ├── Details.razor
│   ├── Delete.razor
│   └── Index.razor
├── CategoryPages/       # Gestion des catégories
├── UserPages/          # Gestion des utilisateurs
└── StatisticPages/     # Statistiques
```

### Layout et navigation

```
R2.UI/Components/Layout/
├── MainLayout.razor     # Layout principal
├── NavMenu.razor       # Menu desktop
└── NavMenuMobile.razor # Menu mobile
```

## Pipeline CI/CD

### Workflow d'intégration (`01_integration.yaml`)

**Déclencheur** : Pull Request vers `main`

**Jobs** :
1. **Test_App** : Exécution des tests
2. **Quality_SonarQube** : Analyse qualité code

### Workflow de déploiement (`02_Deploy.yaml`)

**Déclencheur** : Push vers `main`

**Jobs** :
1. **SnykSecurity** : Analyse sécuritaire des dépendances
2. **Dockerbuild** : Construction et push de l'image Docker
3. **Deploy** : Déploiement sur Azure VM

### Détail des workflows

#### 1. Tests (`01-1_TestApp.yaml`)
```yaml
- Tests unitaires avec filtre Category!=Integration
- Tests d'intégration avec filtre Category=Integration
- Génération de rapports de couverture
- Upload des artifacts (résultats et couverture)
```

#### 2. SonarQube (`01-2_SonarQube.yaml`)
```yaml
- Analyse statique du code
- Intégration avec SonarCloud
- Organisation: bloc3-orga
- Projet: bloc3-orga_RessourcesRelationnelles
```

#### 3. Sécurité (`01-4_Snyk.yaml`)
```yaml
- Scan des dépendances
- Analyse statique du code
- Upload SARIF vers GitHub Security
- Support des seuils de sévérité configurables
```

#### 4. Docker Build (`02-1_Dockerbuild.yaml`)
```yaml
- Construction de l'image Docker
- Push vers GitHub Container Registry (GHCR)
- Scan sécuritaire Docker avec Snyk
- Tag: ghcr.io/{repository}:latest
```

#### 5. Déploiement (`02-2_Deploy.yaml`)
```yaml
- Déploiement Canary sur Azure VM
- Mise à jour docker-compose.yml
- Configuration des variables d'environnement
- Tests de connectivité post-déploiement
```

## Tests

### Structure des tests

```
R2.Tests/
├── AuthPages/           # Tests d'authentification
├── ResourcePages/       # Tests CRUD ressources  
├── Integration/         # Tests d'intégration
├── Context/            # Contexte de test mockés
└── R2.Tests.csproj
```

### Types de tests

#### Tests unitaires
- Tests des composants Blazor avec bUnit
- Mocking avec Moq
- Tests des formulaires et validation
- Tests de navigation

#### Tests d'intégration
- Tests avec base de données en mémoire
- Tests des workflows complets
- Tests d'authentification end-to-end

### Configuration de test
```csharp
// Exemple de configuration test
public class TestDbContext : R2DbContext
{
    public TestDbContext(DbContextOptions<R2DbContext> options) : base(options) { }
    
    // DbSets mockés pour les tests
    public void SetupTestData(List<Category> categories, List<Resource> resources)
    {
        // Configuration des données de test
    }
}
```

## Intégration d'une nouvelle fonctionnalité

### 1. Développement local

#### Création de l'entité (si nécessaire)
```csharp
// R2.Data/Entities/NouvelleEntite.cs
public class NouvelleEntite
{
    public int Id { get; set; }
    public string Propriete { get; set; }
    // ... autres propriétés
}
```

#### Mise à jour du DbContext
```csharp
// R2.Data/Context/R2DbContext.cs
public DbSet<NouvelleEntite> NouvellesEntites { get; set; }
```

#### Migration de base de données
```bash
# Depuis le dossier R2.Data
dotnet ef migrations add AjoutNouvelleEntite
dotnet ef database update
```

### 2. Création des pages Blazor

#### Structure recommandée
```
R2.UI/Components/Pages/NouvelleEntitePages/
├── Create.razor      # Création
├── Edit.razor        # Modification  
├── Details.razor     # Détails
├── Delete.razor      # Suppression
└── Index.razor       # Liste/Index
```

#### Template de composant
```razor
@page "/nouvelles-entites"
@using Microsoft.EntityFrameworkCore
@using R2.Data.Entities
@inject IDbContextFactory<R2DbContext> DbFactory
@inject NavigationManager NavigationManager

<PageTitle>Nouvelles Entités</PageTitle>

<div class="d-flex justify-content-between align-items-center mb-4">
    <h1>Nouvelles Entités</h1>
    <a href="nouvelles-entites/create" class="btn btn-primary">
        <i class="bi bi-plus-circle"></i> Créer
    </a>
</div>

<!-- Contenu du composant -->

@code {
    // Logique du composant
}
```

### 3. Tests

#### Tests unitaires
```csharp
// R2.Tests/NouvelleEntitePages/NouvelleEntiteTests.cs
public class NouvelleEntiteTests : TestContext
{
    [Fact]
    public void Create_ShouldDisplayForm()
    {
        // Arrange & Act
        var cut = RenderComponent<Create>();
        
        // Assert
        Assert.NotNull(cut.Find("form"));
    }
}
```

#### Tests d'intégration
```csharp
[Fact]
public async Task NouvelleEntite_CRUD_Workflow()
{
    // Test du workflow complet Create-Read-Update-Delete
}
```

### 4. Mise à jour de la navigation

#### Menu desktop
```razor
<!-- R2.UI/Components/Layout/NavMenu.razor -->
<div class="nav-item px-3">
    <NavLink class="nav-link" href="nouvelles-entites">
        <i class="bi bi-icon me-2"></i> Nouvelles Entités
    </NavLink>
</div>
```

#### Menu mobile
```razor
<!-- R2.UI/Components/Layout/NavMenuMobile.razor -->
<button type="button" class="nav-item" @onclick="GoToNouvellesEntites">
    <i class="bi bi-icon"></i>
    <span class="nav-label">Entités</span>
</button>
```

### 5. Autorisation (si nécessaire)

```razor
<AuthorizeView Roles="Administrateur,Super-Administrateur">
    <Authorized>
        <!-- Contenu pour administrateurs -->
    </Authorized>
</AuthorizeView>
```

### 6. Workflow de validation

#### 1. Développement en feature branch
```bash
git checkout -b feature/nouvelle-fonctionnalite
# Développement...
git commit -m "feat: ajout nouvelle fonctionnalité"
git push origin feature/nouvelle-fonctionnalite
```

#### 2. Pull Request
- Création de la PR vers `main`
- **Déclenchement automatique du workflow d'intégration** :
  - Exécution des tests unitaires et d'intégration
  - Analyse SonarQube
  - Vérification de la qualité du code

#### 3. Review et merge
- Review par les pairs
- Merge vers `main` après validation

#### 4. Déploiement automatique
- **Déclenchement automatique du workflow de déploiement** :
  - Analyse sécuritaire Snyk
  - Build et push de l'image Docker
  - Déploiement sur l'environnement de production

## Configuration et déploiement

### Variables d'environnement requises

#### Secrets GitHub Actions
```yaml
GHCR_TOKEN          # Token GitHub Container Registry
SONAR_TOKEN         # Token SonarCloud
SNYK_TOKEN          # Token Snyk (optionnel)
AZURE_HOST          # IP/Hostname du serveur Azure
AZURE_LOGIN         # Nom d'utilisateur SSH
AZURE_PORT          # Port SSH (22)
AZURE_PWD           # Mot de passe SSH
DB_PASSWORD         # Mot de passe base de données
```

### Configuration Docker

#### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["R2.UI/R2.UI.csproj", "R2.UI/"]
COPY ["R2.Data/R2.Data.csproj", "R2.Data/"]
RUN dotnet restore "./R2.UI/R2.UI.csproj"

COPY . .
WORKDIR "/src/R2.UI"
RUN dotnet build "./R2.UI.csproj" -c Release -o /app/build
RUN dotnet publish "./R2.UI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "R2.UI.dll"]
```

#### Docker Compose
```yaml
services:
  r2-database:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "${DB_PASSWORD}"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    
  r2-ui:
    image: ghcr.io/username/r2:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=r2-database;...
    ports:
      - "8080:8080"
    depends_on:
      - r2-database
```

## Monitoring et observabilité

### Métriques de déploiement
- **SLA Target** : < 2 secondes de temps de réponse
- **Monitoring** : Performance, taux d'erreur, disponibilité
- **Health Checks** : Vérification de connectivité base de données

### Logging
- Logs structurés avec les niveaux appropriés
- Gestion des erreurs avec try-catch
- Logs de débogage pour le développement

## Bonnes pratiques

### Développement
1. **Suivre la convention de nommage** établie dans le projet
2. **Tests** : Écrire des tests pour chaque nouvelle fonctionnalité
3. **Sécurité** : Utiliser `[Authorize]` pour protéger les endpoints sensibles
4. **Responsive Design** : Tester sur mobile et desktop
5. **Validation** : Utiliser DataAnnotations pour la validation des formulaires

### Git et CI/CD
1. **Feature branches** : Développer dans des branches séparées
2. **Commits atomiques** : Un commit = une fonctionnalité/fix
3. **Messages de commit** : Suivre la convention (feat:, fix:, docs:, etc.)
4. **Pull Requests** : Toujours passer par une PR pour valider le code

### Sécurité
1. **Secrets** : Ne jamais commiter de secrets dans le code
2. **HTTPS** : Toujours utiliser HTTPS en production
3. **Input Validation** : Valider toutes les entrées utilisateur
4. **Autorisation** : Vérifier les permissions à chaque action sensible

---

*Cette documentation est maintenue à jour avec les évolutions du projet R2.*