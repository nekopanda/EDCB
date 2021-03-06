﻿function ConvertTitle(title)
  local title=string.gsub(title, '%[(新)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(終)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(再)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(交)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(映)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(手)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(声)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(多)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(字)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(二)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(Ｓ)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(Ｂ)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(SS)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(無)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(Ｃ)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(S1)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(S2)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(S3)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(MV)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(双)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(デ)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(Ｄ)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(Ｎ)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(Ｗ)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(Ｐ)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(HV)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(SD)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(天)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(解)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(料)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(前)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(後)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(初)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(生)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(販)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(吹)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(PPV)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(演)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(移)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(他)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '%[(収)%]', '<span class="mark">%1</span>')
  title=string.gsub(title, '　', ' ')
  return title
end


function ConvertSearch(title)
  local title=string.gsub (title, '　', '+')
  title=string.gsub (title, "/", "_")
  title=string.gsub (title, "＜.-＞", "")
  title=string.gsub (title, "【.-】", "")
  title=string.gsub (title, "%[.-%]", "")
  title=string.gsub (title, "（.-版）", "")
  local search='<a href="search.html?andkey='..title..'"><i class="material-icons">search</i></a>'
  search=search..'<a href="https://www.google.co.jp/search?q='..title..'" target="_blank"><img class="icon" src="img/google.png" alt="Google検索"></a>'
  --search=search..'<a href="http://www.google.co.jp/search?q='..title..'&btnI=Im+Feeling+Lucky" target="_blank">◎</a>'
  return search
end
