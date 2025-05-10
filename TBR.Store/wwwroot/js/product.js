$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: { url: '/admin/product/GetAll'},
        "columns": [
            { data: 'title',"width":"25%"},
            { data: 'describtion', "width": "15%" },
            { data: 'isbn', "width": "10%"},
            { data: 'author', "width": "20%" },
            { data: 'displayPrice', "width": "15%" },
            { data: 'categoryName', "width": "10%" },  
            {

             data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                <a href="/admin/product/edit?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i>Edit</a>
                <a href="/admin/product/delete?id=${data}" class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i>Delete</a>
            </div>`
                },
                "width": "10%"
            }

            
        ]
    });
}
                   