# API Flows

## High-Level API Flow

```mermaid
%%{init: {'theme':'base', 'themeVariables': { 'primaryColor':'#e8f5e9','secondaryColor':'#fff3e0','tertiaryColor':'#e1f5fe', 'lineColor':'#81c784'}, 'themeCSS': '.flowchart-link { stroke-width: 2.5px !important; }'}}%%

flowchart LR
    A[BerryFilterOptions] --> B[PoffinSearch.Run]
    C[PoffinSearchOptions] --> B
    D[PoffinFilterOptions] --> B
    B --> E[TopK PoffinResult]
    E --> F[Result array]

    style A fill:#fce4ec,stroke:#c2185b,stroke-width:2px
    style B fill:#b2dfdb,stroke:#00695c,stroke-width:2px
    style C fill:#e8eaf6,stroke:#283593,stroke-width:2px
    style D fill:#fce4ec,stroke:#ad1457,stroke-width:2px
    style E fill:#e0f2f1,stroke:#00897b,stroke-width:2px
    style F fill:#fff9c4,stroke:#f9a825,stroke-width:2px
```
---

## Berry API Flow

```mermaid
%%{init: {'theme':'base', 'themeVariables': { 'primaryColor':'#e8f5e9','secondaryColor':'#fff3e0','tertiaryColor':'#e1f5fe', 'lineColor':'#81c784'}, 'themeCSS': '.flowchart-link { stroke-width: 2.5px !important; }'}}%%

flowchart LR
    A[BerryTable.All] --> B[BerryQuery.Execute]
    C[BerryFilterOptions] --> B
    D[BerrySortKey array] --> B
    B --> E[Span Berry results]

    style A fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px
    style B fill:#e0f7fa,stroke:#00838f,stroke-width:2px
    style C fill:#fce4ec,stroke:#c2185b,stroke-width:2px
    style D fill:#f1f8e9,stroke:#689f38,stroke-width:2px
    style E fill:#e1f5fe,stroke:#0277bd,stroke-width:2px
```
---

## Poffin Search Workflow

```mermaid
%%{init: {'theme':'base', 'themeVariables': { 'primaryColor':'#e8f5e9','secondaryColor':'#fff3e0','tertiaryColor':'#e1f5fe', 'lineColor':'#81c784'}, 'themeCSS': '.flowchart-link { stroke-width: 2.5px !important; }'}}%%

flowchart TD
    A[User selects berries] --> B[BerryQuery filter]
    B --> C{Filter empty?}
    C -->|Yes| D[PoffinComboTable all combos]
    C -->|No| E[Subset combos enumerate]
    D --> F{Parallel threshold met?}
    E --> F
    F -->|Yes| G[Cook in parallel]
    F -->|No| H[Cook sequentially]
    G --> I[PoffinFilterOptions]
    H --> I
    I --> J[Score + TopK]
    J --> K[Results]

    style A fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    style B fill:#e0f7fa,stroke:#00838f,stroke-width:2px
    style C fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    style D fill:#d1c4e9,stroke:#673ab7,stroke-width:2px
    style E fill:#c5cae9,stroke:#3949ab,stroke-width:2px
    style F fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    style G fill:#ffccbc,stroke:#d84315,stroke-width:2px
    style H fill:#ffccbc,stroke:#d84315,stroke-width:2px
    style I fill:#fce4ec,stroke:#ad1457,stroke-width:2px
    style J fill:#e0f2f1,stroke:#00897b,stroke-width:2px
    style K fill:#fff9c4,stroke:#f9a825,stroke-width:2px
```
---

## Internal Cooking Flow

```mermaid
%%{init: {'theme':'base', 'themeVariables': { 'primaryColor':'#e8f5e9','secondaryColor':'#fff3e0','tertiaryColor':'#e1f5fe', 'lineColor':'#81c784'}, 'themeCSS': '.flowchart-link { stroke-width: 2.5px !important; }'}}%%

flowchart TD
    A[PoffinSearch.Run] --> B{All berries?}
    B -->|Yes| C[PoffinComboTable slice]
    B -->|No| D[BerryQuery + subset ids]
    C --> E[PoffinCooker.Cook combo]
    D --> F[PoffinCooker.Cook span]
    E --> G[Filter PoffinFilterOptions]
    F --> G
    G --> H[Score PoffinScoreOptions]
    H --> I[TopK merge]

    style A fill:#b2dfdb,stroke:#00695c,stroke-width:2px
    style B fill:#fff3e0,stroke:#f57c00,stroke-width:2px
    style C fill:#d1c4e9,stroke:#673ab7,stroke-width:2px
    style D fill:#e0f7fa,stroke:#00838f,stroke-width:2px
    style E fill:#ffccbc,stroke:#d84315,stroke-width:2px
    style F fill:#ffccbc,stroke:#d84315,stroke-width:2px
    style G fill:#fce4ec,stroke:#ad1457,stroke-width:2px
    style H fill:#e0f2f1,stroke:#00897b,stroke-width:2px
    style I fill:#e0f2f1,stroke:#00897b,stroke-width:2px
```

---

## Color Legend

| Color | Component Type | Examples |
|-------|---------------|----------|
| **Red** | Enums & Filters | `Flavor`, `BerryFilterOptions`, `PoffinFilterOptions` |
| **Blue** | Core Data Models | `Berry`, `BerryId` |
| **Purple** | Raw Data & Precomputed | `BerryBase`, `PoffinComboBase`, `PoffinComboTable` |
| **Green** | Repositories & Services | `BerryTable`, `BerryQuery`, `PoffinSearch` |
| **Orange** | Cooking Engine | `PoffinCooker` |
| **Yellow** | Results & Products | `Poffin`, `PoffinResult` |
| **Teal** | Utilities & Data Structures | `TopK`, `BerrySorter` |

---

## Key Components

### Berry System
- **BerryTable**: Central repository providing access to all berry data
- **Berry**: Computed model with derived flavor properties
- **BerryBase**: Raw data used for cooking calculations
- **BerryQuery**: Filtering and sorting service

### Poffin System
- **PoffinCooker**: Core cooking engine with configurable parameters
- **PoffinComboTable**: Precomputed combinations for optimization
- **PoffinSearch**: Orchestrates the entire search and ranking process
- **TopK**: Efficient data structure for maintaining best results

---

### Optimization Strategy
The system uses a dual-path approach:
1. **All berries**: Uses precomputed `PoffinComboTable` for maximum speed
2. **Filtered berries**: Generates combinations on-demand via `PoffinComboEnumerator`
