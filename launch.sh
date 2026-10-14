set -e
set -x

script_dir=$(dirname $(realpath $0))
mod_dir=$script_dir
game_dir=$(realpath ~/.local/share/Steam/steamapps/common/RimWorld/)
pushd $mod_dir

LC_ALL=C steam-run $game_dir/RimWorld -logfile $mod_dir/log/rimworld.log -popupwindow $@

popd
