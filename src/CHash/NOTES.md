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


# Запуск диспетчера
`./CLI start -p 8080 -b -d ../../../../Dispatcher/bin/Debug/net9.0/Dispatcher`

# Запуск узла
`./CLI add -p 8080 -t ../../../../HashingNode/bin/Debug/net9.0/HashingNode -h 8081`

# Удаление ноды хэширования
`./CLI remove --node-port 5001 --port 8080`

# Получение нод хэширования
`./CLI list --port 8080`