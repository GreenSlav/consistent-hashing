- Настроить dev сертификаты для того, чтоб GRPC мог работать
- В .csproj проектов стоит указывать 
`
<ItemGroup>
    <Protobuf Include="<Путь_к_proto_файлу>" GrpcServices="<Роль_проекта>" />
</ItemGroup>
`

Роли:
- None (проект, содержащий .proto файлы, но не участвующий в коммуникации)
- Server
- Client