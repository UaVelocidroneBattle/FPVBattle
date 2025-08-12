## Api
Project uses openapi specification to access Velocidrone bot. Api access layer is generated from specification file, that is located by location: `shared/api/Veloci.Web.json`.
To generate wrapper please run following command:

```bash
npm run gen
```

To run local dev server:

```bash
npm run dev
```

To build the project for production:

```bash
npm run build
```