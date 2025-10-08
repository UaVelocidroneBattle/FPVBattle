# VelocidroneBot Documentation

This directory contains all project documentation including architectural decision records.

## Documentation Structure

```
docs/
├── README.md                 # This file - documentation index
├── adr/                      # Architectural Decision Records
│   ├── 0001-vertical-slices-architecture.md
│   ├── 0002-zustand-for-state-management.md
│   └── template.md           # ADR template
```

## Quick Links

### For Developers
- [Local Development Setup](deployment/local-development.md)
- [Architectural Decision Records](adr/)
- [API Documentation](api/endpoints.md)

## Contributing to Documentation

### Adding New Documentation
1. Follow the existing structure
2. Use clear, descriptive filenames
3. Include a brief summary at the top of each document
4. Update this README when adding new sections

### Architectural Decision Records (ADRs)
When making significant architectural decisions:
1. Copy `adr/template.md` to a new file
2. Use sequential numbering: `0001-`, `0002-`, etc.
3. Use kebab-case for the title
4. Update the decision status as needed

### Documentation Standards
- Use Markdown format for all documentation
- Include code examples where helpful
- Keep documentation close to the code it describes
- Review and update documentation with code changes