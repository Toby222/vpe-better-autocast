set -e
set -x

script_dir=$(dirname $(realpath $0))
mod_dir=$(dirname $script_dir)
pushd $mod_dir

dotnet tool restore
dotnet roslynator analyze $script_dir/mod.csproj

popd
